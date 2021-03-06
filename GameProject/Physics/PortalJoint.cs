﻿using System;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Dynamics;
using FarseerPhysics;
using Game.Common;
using Game.Portals;

namespace Game.Physics
{
    // Point-to-point constraint
    // C = p2 - p1
    // Cdot = v2 - v1
    //      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
    // J = [-I -r1_skew I r2_skew ]
    // Identity used:
    // w k % (rx i + ry j) = w * (-ry i + rx j)

    // Angle constraint
    // C = angle2 - angle1 - referenceAngle
    // Cdot = w2 - w1
    // J = [0 0 -1 0 0 1]
    // K = invI1 + invI2

    /// <summary>
    /// A weld joint essentially glues two bodies together. A weld joint may
    /// distort somewhat because the island constraint solver is approximate.
    /// 
    /// The joint is soft constraint based, which means the two bodies will move
    /// relative to each other, when a force is applied. To combine two bodies
    /// in a rigid fashion, combine the fixtures to a single body instead.
    /// </summary>
    public class PortalJoint : Joint
    {
        public IPortal PortalEnter;
         
        // Solver shared
        Vector3 _impulse;
        float _gamma;
        float _bias;

        // Solver temp
        int _indexA;
        int _indexB;
        Vector2 _rA;
        Vector2 _rB;
        Vector2 _localCenterA;
        Vector2 _localCenterB;
        float _invMassA;
        float _invMassB;
        float _invIa;
        float _invIb;
        Mat33 _mass;

        internal PortalJoint()
        {
            JointType = JointType.Portal;
        }

        /// <summary>
        /// You need to specify an anchor point where they are attached.
        /// The position of the anchor point is important for computing the reaction torque.
        /// </summary>
        /// <param name="parentBody">The first body</param>
        /// <param name="childBody">The second body</param>
        /// <param name="portalEnter"></param>
        public PortalJoint(Body parentBody, Body childBody, IPortal portalEnter)
            : base(parentBody, childBody)
        {
            JointType = JointType.Portal;

            PortalEnter = portalEnter;
            LocalAnchorA = new Vector2(0, 0);
            LocalAnchorB = new Vector2(0, 0);

            CollideConnected = true;
        }

        /// <summary>
        /// The local anchor point on BodyA
        /// </summary>
        public Vector2 LocalAnchorA { get; set; }

        /// <summary>
        /// The local anchor point on BodyB
        /// </summary>
        public Vector2 LocalAnchorB { get; set; }

        public override Vector2 WorldAnchorA
        {
            get { return BodyA.GetWorldPoint(LocalAnchorA); }
            set { LocalAnchorA = BodyA.GetLocalPoint(value); }
        }

        public override Vector2 WorldAnchorB
        {
            get { return BodyB.GetWorldPoint(LocalAnchorB); }
            set { LocalAnchorB = BodyB.GetLocalPoint(value); }
        }

        /// <summary>
        /// The frequency of the joint. A higher frequency means a stiffer joint, but
        /// a too high value can cause the joint to oscillate.
        /// Default is 0, which means the joint does no spring calculations.
        /// </summary>
        public float FrequencyHz { get; set; }

        /// <summary>
        /// The damping on the joint. The damping is only used when
        /// the joint has a frequency (> 0). A higher value means more damping.
        /// </summary>
        public float DampingRatio { get; set; }

        public override Vector2 GetReactionForce(float invDt)
        {
            return invDt * new Vector2(_impulse.X, _impulse.Y);
        }

        public override float GetReactionTorque(float invDt)
        {
            return invDt * _impulse.Z;
        }

        void TransformOrientation(SolverData data)
        {
            int indexB = BodyB.IslandIndex;

            Position positionB = data.positions[indexB];
            Velocity velocityB = data.velocities[indexB];

            var t = Portal.Enter(PortalEnter.Linked, new Transform2(positionB.c, 1, positionB.a));
            var v = Portal.EnterVelocity(PortalEnter.Linked, 0.5f, new Transform2(velocityB.v, 1, velocityB.w));

            data.positions[indexB].c = (Vector2)t.Position;
            data.positions[indexB].a = t.Rotation;

            data.velocities[indexB].v = (Vector2)v.Position;
            data.velocities[indexB].w = v.Rotation;
        }

        void UndoTransformOrientation(SolverData data)
        {
            int indexB = BodyB.IslandIndex;

            Position positionB = data.positions[indexB];
            Velocity velocityB = data.velocities[indexB];

            var t = Portal.Enter(PortalEnter, new Transform2(positionB.c, 1, positionB.a));
            var v = Portal.EnterVelocity(PortalEnter, 0.5f, new Transform2(velocityB.v, 1, velocityB.w));

            data.positions[indexB].c = (Vector2)t.Position;
            data.positions[indexB].a = t.Rotation;

            data.velocities[indexB].v = (Vector2)v.Position;
            data.velocities[indexB].w = v.Rotation;
        }

        public override void InitVelocityConstraints(ref SolverData data)
        {
            TransformOrientation(data);

            _indexA = BodyA.IslandIndex;
            _indexB = BodyB.IslandIndex;
            _localCenterA = BodyA._sweep.LocalCenter;
            _localCenterB = BodyB._sweep.LocalCenter;
            _invMassA = BodyA._invMass;
            _invMassB = BodyB._invMass;
            _invIa = BodyA._invI;
            _invIb = BodyB._invI;

            float aA = data.positions[_indexA].a;
            Vector2 vA = data.velocities[_indexA].v;
            float wA = data.velocities[_indexA].w;

            float aB = data.positions[_indexB].a;
            Vector2 vB = data.velocities[_indexB].v;
            float wB = data.velocities[_indexB].w;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            _rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            _rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);

            // J = [-I -r1_skew I r2_skew]
            //     [ 0       -1 0       1]
            // r_skew = [-ry; rx]

            // Matlab
            // K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
            //     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
            //     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIa, iB = _invIb;

            Mat33 K = new Mat33();
            K.ex.X = mA + mB + _rA.Y * _rA.Y * iA + _rB.Y * _rB.Y * iB;
            K.ey.X = -_rA.Y * _rA.X * iA - _rB.Y * _rB.X * iB;
            K.ez.X = -_rA.Y * iA - _rB.Y * iB;
            K.ex.Y = K.ey.X;
            K.ey.Y = mA + mB + _rA.X * _rA.X * iA + _rB.X * _rB.X * iB;
            K.ez.Y = _rA.X * iA + _rB.X * iB;
            K.ex.Z = K.ez.X;
            K.ey.Z = K.ez.Y;
            K.ez.Z = iA + iB;

            if (FrequencyHz > 0.0f)
            {
                K.GetInverse22(ref _mass);

                float invM = iA + iB;
                float m = invM > 0.0f ? 1.0f / invM : 0.0f;

                float c = aB - aA;

                // Frequency
                float omega = 2.0f * Settings.Pi * FrequencyHz;

                // Damping coefficient
                float d = 2.0f * m * DampingRatio * omega;

                // Spring stiffness
                float k = m * omega * omega;

                // magic formulas
                float h = data.step.dt;
                _gamma = h * (d + h * k);
                _gamma = _gamma != 0.0f ? 1.0f / _gamma : 0.0f;
                _bias = c * h * k * _gamma;

                invM += _gamma;
                _mass.ez.Z = invM != 0.0f ? 1.0f / invM : 0.0f;
            }
            else
            {
                K.GetSymInverse33(ref _mass);
                _gamma = 0.0f;
                _bias = 0.0f;
            }

            if (Settings.EnableWarmstarting)
            {
                // Scale impulses to support a variable time step.
                _impulse *= data.step.dtRatio;

                Vector2 p = new Vector2(_impulse.X, _impulse.Y);

                vA -= mA * p;
                wA -= iA * (MathUtils.Cross(_rA, p) + _impulse.Z);

                vB += mB * p;
                wB += iB * (MathUtils.Cross(_rB, p) + _impulse.Z);
            }
            else
            {
#pragma warning disable CS0162 // Unreachable code detected
                _impulse = Vector3.Zero;
#pragma warning restore CS0162 // Unreachable code detected
            }

            data.velocities[_indexA].v = vA;
            data.velocities[_indexA].w = wA;
            data.velocities[_indexB].v = vB;
            data.velocities[_indexB].w = wB;

            UndoTransformOrientation(data);
        }

        public override void SolveVelocityConstraints(ref SolverData data)
        {
            TransformOrientation(data);

            Vector2 vA = data.velocities[_indexA].v;
            float wA = data.velocities[_indexA].w;
            Vector2 vB = data.velocities[_indexB].v;
            float wB = data.velocities[_indexB].w;

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIa, iB = _invIb;

            if (FrequencyHz > 0.0f)
            {
                float cdot2 = wB - wA;

                float impulse2 = -_mass.ez.Z * (cdot2 + _bias + _gamma * _impulse.Z);
                _impulse.Z += impulse2;

                wA -= iA * impulse2;
                wB += iB * impulse2;

                Vector2 cdot1 = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);

                Vector2 impulse1 = -MathUtils.Mul22(_mass, cdot1);
                _impulse.X += impulse1.X;
                _impulse.Y += impulse1.Y;

                Vector2 p = impulse1;

                vA -= mA * p;
                wA -= iA * MathUtils.Cross(_rA, p);

                vB += mB * p;
                wB += iB * MathUtils.Cross(_rB, p);
            }
            else
            {
                Vector2 cdot1 = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);
                float cdot2 = wB - wA;
                Vector3 cdot = new Vector3(cdot1.X, cdot1.Y, cdot2);

                Vector3 impulse = -MathUtils.Mul(_mass, cdot);
                _impulse += impulse;

                Vector2 p = new Vector2(impulse.X, impulse.Y);

                vA -= mA * p;
                wA -= iA * (MathUtils.Cross(_rA, p) + impulse.Z);

                vB += mB * p;
                wB += iB * (MathUtils.Cross(_rB, p) + impulse.Z);
            }

            data.velocities[_indexA].v = vA;
            data.velocities[_indexA].w = wA;
            data.velocities[_indexB].v = vB;
            data.velocities[_indexB].w = wB;

            UndoTransformOrientation(data);
        }

        public override bool SolvePositionConstraints(ref SolverData data)
        {
            TransformOrientation(data);

            Vector2 cA = data.positions[_indexA].c;
            float aA = data.positions[_indexA].a;
            Vector2 cB = data.positions[_indexB].c;
            float aB = data.positions[_indexB].a;

            Rot qA = new Rot(aA), qB = new Rot(aB);

            float mA = _invMassA, mB = _invMassB;
            float iA = _invIa, iB = _invIb;

            Vector2 rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
            Vector2 rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);

            float positionError, angularError;

            Mat33 k = new Mat33();
            k.ex.X = mA + mB + rA.Y * rA.Y * iA + rB.Y * rB.Y * iB;
            k.ey.X = -rA.Y * rA.X * iA - rB.Y * rB.X * iB;
            k.ez.X = -rA.Y * iA - rB.Y * iB;
            k.ex.Y = k.ey.X;
            k.ey.Y = mA + mB + rA.X * rA.X * iA + rB.X * rB.X * iB;
            k.ez.Y = rA.X * iA + rB.X * iB;
            k.ex.Z = k.ez.X;
            k.ey.Z = k.ez.Y;
            k.ez.Z = iA + iB;

            if (FrequencyHz > 0.0f)
            {
                Vector2 c1 = cB + rB - cA - rA;

                positionError = c1.Length();
                angularError = 0.0f;

                Vector2 p = -k.Solve22(c1);

                cA -= mA * p;
                aA -= iA * MathUtils.Cross(rA, p);

                cB += mB * p;
                aB += iB * MathUtils.Cross(rB, p);
            }
            else
            {
                Vector2 c1 = cB + rB - cA - rA;
                float c2 = aB - aA;

                positionError = c1.Length();
                angularError = Math.Abs(c2);

                Vector3 c = new Vector3(c1.X, c1.Y, c2);

                Vector3 impulse = -k.Solve33(c);
                Vector2 p = new Vector2(impulse.X, impulse.Y);

                cA -= mA * p;
                aA -= iA * (MathUtils.Cross(rA, p) + impulse.Z);

                cB += mB * p;
                aB += iB * (MathUtils.Cross(rB, p) + impulse.Z);
            }

            data.positions[_indexA].c = cA;
            data.positions[_indexA].a = aA;
            data.positions[_indexB].c = cB;
            data.positions[_indexB].a = aB;

            UndoTransformOrientation(data);

            return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
        }
    }
}
