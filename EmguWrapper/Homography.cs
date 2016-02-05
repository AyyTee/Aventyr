using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using OpenTK;
using System.Drawing;

namespace EmguWrapper
{
    public static class Homography
    {
        /*public static Matrix3d FindHomography(Vector2[] source, Vector2[] destination)
        {
            HomographyMatrix mat = CameraCalibration.FindHomography(ConvertToPointF(source), ConvertToPointF(destination), Emgu.CV.CvEnum.HomographyMethod.Default);
            Matrix3d matClone = new Matrix3d();
            for (int i = 0; i < 3; i++)
            {
                matClone.Row0 = new Vector3d(mat.Data[0, i], mat.Data[1, i], mat.Data[2, i]);
            }
            return matClone;
        }*/

        public static PointF[] ConvertToPointF(Vector2[] vectors)
        {
            PointF[] points = new PointF[vectors.Length];
            for (int i = 0; i < vectors.Length; i++)
            {
                points[i] = new PointF(vectors[i].X, vectors[i].Y);
            }
            return points;
        }

        public static Matrix4d FindHomography(Vector2[] src, Vector2[] dest, bool lockZ = false)
        {
            double[] _dstMat = new double[16];
            double[] _srcMat = new double[16];
            double[] _warpMat = new double[16];
            ComputeSquareToQuad(dest[0].X, dest[0].Y, dest[1].X, dest[1].Y, dest[2].X, dest[2].Y, dest[3].X, dest[3].Y, _dstMat);
            ComputeQuadToSquare(src[0].X, src[0].Y, src[1].X, src[1].Y, src[2].X, src[2].Y, src[3].X, src[3].Y, _srcMat);
            MultiplyMatrices(_srcMat, _dstMat, _warpMat);
            Matrix4d mat = new Matrix4d();
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    mat[i, j] = _warpMat[i * 4 + j]; 
                }
            }
            /*mat.Row2 = mat.Row3;
            mat.Row3 = Matrix4d.Identity.Row3;*/
            if (!lockZ)
            {
                mat.Column2 = mat.Column3;
                mat.Column3 = Matrix4d.Identity.Column3;
            }
            //*/
            /*HomographyMatrix mat2 = CameraCalibration.FindHomography(ConvertToPointF(src), ConvertToPointF(dest), Emgu.CV.CvEnum.HOMOGRAPHY_METHOD.DEFAULT, 0.001);
            Matrix3d matClone = new Matrix3d();
            for (int i = 0; i < 3; i++)
            {
                matClone.Row0 = new Vector3d(mat2.Data[0, i], mat2.Data[1, i], mat2.Data[2, i]);
            }*/

            return mat;
        }

        /*public void ComputeWarp()
        {
            ComputeSquareToQuad(_dstX[0], _dstY[0], _dstX[1], _dstY[1], _dstX[2], _dstY[2], _dstX[3], _dstY[3], _dstMat);
            ComputeQuadToSquare(_srcX[0], _srcY[0], _srcX[1], _srcY[1], _srcX[2], _srcY[2], _srcX[3], _srcY[3], _srcMat);
            MultiplyMatrices(_srcMat, _dstMat, _warpMat);
            _dirty = false;
        }*/

        public static void MultiplyMatrices(double[] srcMat, double[] dstMat, double[] resMat)
        {
            for (int row = 0; row < 4; row++)
            {
                int rowIndex = row * 4;
                for (int col = 0; col < 4; col++)
                {
                    resMat[rowIndex + col] = (srcMat[rowIndex] * dstMat[col] +
                                                srcMat[rowIndex + 1] * dstMat[col + 4] +
                                                srcMat[rowIndex + 2] * dstMat[col + 8] +
                                                srcMat[rowIndex + 3] * dstMat[col + 12]);
                }
            }
        }

        public static void ComputeSquareToQuad(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3, double[] mat)
        {
	        double dx1 = x1 - x2,	dy1 = y1 - y2;
	        double dx2 = x3 - x2,	dy2 = y3 - y2;
	        double sx = x0 - x1 + x2 - x3;
	        double sy = y0 - y1 + y2 - y3;
	        double g = (sx * dy2 - dx2 * sy) / (dx1 * dy2 - dx2 * dy1);
	        double h = (dx1 * sy - sx * dy1) / (dx1 * dy2 - dx2 * dy1);
	        double a = x1 - x0 + g * x1;
	        double b = x3 - x0 + h * x3;
	        double c = x0;
	        double d = y1 - y0 + g * y1;
	        double e = y3 - y0 + h * y3;
	        double f = y0;

	        mat[0] = a;		mat[1] = d;		mat[2] = 0;		mat[3] = g;
	        mat[4] = b;		mat[5] = e;		mat[6] = 0;		mat[7] = h;
	        mat[8] = 0;		mat[9] = 0;		mat[10] = 1;	mat[11] = 0;
            mat[12] = c; mat[13] = f; mat[14] = 0; mat[15] = 1;
        }

        public static void ComputeQuadToSquare(double x0, double y0, double x1, double y1, double x2, double y2, double x3, double y3, double[] mat)
        {
            ComputeSquareToQuad(x0, y0, x1, y1, x2, y2, x3, y3, mat);

            //invert through adjoint
            double a = mat[0], d = mat[1],	/*ignore*/ 	g = mat[3];
            double b = mat[4], e = mat[5], /*3rd col*/	h = mat[7];
            /*ignore 3rd row*/
            double c = mat[12], f = mat[13];

            double a1 = e - f * h;
            double b1 = c * h - b;
            double c1 = b * f - c * e;
            double d1 = f * g - d;
            double e1 = a - c * g;
            double f1 = c * d - a * f;
            double g1 = d * h - e * g;
            double h1 = b * g - a * h;
            double i1 = a * e - b * d;

            double idet = 1.0f / (a * a1 + b * d1 + c * g1);

            mat[0] = a1 * idet; mat[1] = d1 * idet; mat[2] = 0; mat[3] = g1 * idet;
            mat[4] = b1 * idet; mat[5] = e1 * idet; mat[6] = 0; mat[7] = h1 * idet;
            mat[8] = 0; mat[9] = 0; mat[10] = 1; mat[11] = 0;
            mat[12] = c1 * idet; mat[13] = f1 * idet; mat[14] = 0; mat[15] = i1 * idet;
        }

        /*public double[] GetWarpMatrix()
        {
            return _warpMat;
        }*/
    }
}
