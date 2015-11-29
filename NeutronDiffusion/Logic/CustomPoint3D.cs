using System;
using System.Windows.Media.Media3D;

namespace NeutronDiffusion.Logic
{
    public class CustomPoint3D : IEquatable<CustomPoint3D>
    {
        private const int Precise = 4;

        public double X
        {
            get { return Vector.X; }
            set { Vector.X = value; }
        }

        public double Y
        {
            get { return Vector.Y; }
            set { Vector.Y = value; }
        }

        public double Z
        {
            get { return Vector.Z; }
            set { Vector.Z = value; }
        }

        public Vector3D Vector;

        private const double Eps = 0.000001;

        public CustomPoint3D(double x, double y, double z)
        {
            Vector = new Vector3D(Math.Round(x, Precise), Math.Round(y, Precise), Math.Round(z, Precise));
        }

        public CustomPoint3D(Vector3D v)
        {
            Vector = v;
        }

        public CustomPoint3D()
        {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public CustomPoint3D(CustomPoint3D p)
        {
            Init(p);
        }

        public double[] ToDoubleArray()
        {
            return new[] {X, Y, Z};
        }

        public bool Equals(CustomPoint3D p)
        {
            return (Math.Abs(X - p.X) < Eps &&
                    Math.Abs(Y - p.Y) < Eps &&
                    Math.Abs(Z - p.Z) < Eps);
        }

        public double DistanceTo(CustomPoint3D p)
        {
            var res = p - this;
            return Math.Sqrt(res*res);
        }

        public CustomPoint3D Add(CustomPoint3D p)
        {
            X += p.X;
            Y += p.Y;
            Z += p.Z;

            return this;
        }

        public CustomPoint3D Multiply(float s)
        {
            X *= s;
            Y *= s;
            Z *= s;

            return this;
        }

        public CustomPoint3D Multiply(double s)
        {
            X *= (float) s;
            Y *= (float) s;
            Z *= (float) s;

            return this;
        }

        public void Init(CustomPoint3D p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }

        public override String ToString()
        {
            return $"{{{X,8:0.00000}, {Y,8:0.00000}, {Z,8:0.00000}}}";
        }

        public static CustomPoint3D operator -(CustomPoint3D p1, CustomPoint3D p2)
        {
            return new CustomPoint3D(p1.X - p2.X, p1.Y - p2.Y, p1.Z - p2.Z);
        }

        public static CustomPoint3D operator +(CustomPoint3D p1, CustomPoint3D p2)
        {
            return new CustomPoint3D(p1.X + p2.X, p1.Y + p2.Y, p1.Z + p2.Z);
        }

        public static double operator *(CustomPoint3D p1, CustomPoint3D p2)
        {
            return p1.X*p2.X + p1.Y*p2.Y + p1.Z*p2.Z;
        }

        public static CustomPoint3D operator *(float s, CustomPoint3D p)
        {
            return new CustomPoint3D(s*p.X, s*p.Y, s*p.Z);
        }

        public static CustomPoint3D operator *(double s, CustomPoint3D p)
        {
            return new CustomPoint3D(s*p.X, s*p.Y, s*p.Z);
        }

        public static CustomPoint3D operator *(CustomPoint3D p, float s)
        {
            return new CustomPoint3D(s*p.X, s*p.Y, s*p.Z);
        }

        public static CustomPoint3D operator *(CustomPoint3D p, double s)
        {
            return new CustomPoint3D(s*p.X, s*p.Y, s*p.Z);
        }

        public static CustomPoint3D operator /(CustomPoint3D p, float s)
        {
            return new CustomPoint3D(p.X/s, p.Y/s, p.Z/s);
        }

        public static CustomPoint3D operator /(CustomPoint3D p, double s)
        {
            return new CustomPoint3D(p.X/s, p.Y/s, p.Z/s);
        }

        public static double DistanceBetween(CustomPoint3D p1, CustomPoint3D p2)
        {
            return p1.DistanceTo(p2);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X.GetHashCode();
                hashCode = (hashCode*397) ^ Y.GetHashCode();
                hashCode = (hashCode*397) ^ Z.GetHashCode();
                return hashCode;
            }
        }
    }
}