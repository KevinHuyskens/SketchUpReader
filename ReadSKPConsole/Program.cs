using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SketchUpNET;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;

namespace ReadSKPConsole
{
    class Program
    {
        #region main
        static void Main(string[] args)
        {
            if (args == null || args.Length < 2) return;
            if (loadSkp(args[1]))
                SendVertsToUnity(args[0]);
        }
        #endregion

        #region variables
        private static Vector3 vertice = new Vector3();
        private static List<string> faceStrings = new List<string>();
        #endregion

        #region Getting the .SKP data
        private static bool loadSkp(string path)
        {
            SketchUp skp = new SketchUp();
            if (skp.LoadModel(path, true))
            {
                foreach (Instance i in skp.Instances)
                {
                    foreach (Surface s in skp.Surfaces)
                    {
                        GetSurfaceVertices(s, new List<Transform>() { i.Transformation });
                    }

                    DrawInstance(i, new List<Transform>());
                }
                foreach (Group g in skp.Groups)
                {
                    DrawGroup(g, new List<Transform>());
                }

                if (skp.Instances.Count == 0)
                {
                    foreach (KeyValuePair<string, SketchUpNET.Component> c in skp.Components)
                    {
                        DrawComponent(c.Value, new List<Transform>());
                    }
                }

                faceStrings.Add("s");
                foreach (Surface s in skp.Surfaces)
                { 
                    GetSurfaceVertices(s);
                }
            }
            return true;
        }
        #endregion
        
        #region Pipe client
        private static void SendVertsToUnity(string processName)
        {
            NamedPipeClientStream client = new NamedPipeClientStream(".", processName,
               PipeDirection.InOut, PipeOptions.None,
               TokenImpersonationLevel.Impersonation);

            //Connect to server
            client.Connect();
            //Created stream for reading and writing
            StreamString clientStream = new StreamString(client);
            //Send Message to Server
            foreach(string s in faceStrings)
            {
                clientStream.WriteString(s);
            }

            clientStream.WriteString("END");
            //Close client
            client.Close();
        }
        #endregion

        #region Drawing functions
        private static void DrawGroup(Group g, List<Transform> t)
        {
            t.Add(g.Transformation);
            // draw instances 
            foreach (Instance i in g.Instances)
            {
                DrawInstance(i, t);
            }
            // draw groups
            foreach (Group childG in g.Groups)
            {
                DrawGroup(childG, t);
            }

            // done clear trans
            t.RemoveAt(t.Count - 1);
        }

        private static void DrawInstance(Instance i, List<Transform> t)
        {
            t.Add(i.Transformation);

            // draw instance 'mesh

            if (i.Parent == null)
            {
                t.RemoveAt(t.Count - 1);
                return;
            }
            DrawComponent(i.Parent as Component, t);

            // done clear trans
            t.RemoveAt(t.Count - 1);
        }

        private static void DrawComponent(Component c, List<Transform> t)
        {
            faceStrings.Add("s");
            // draw surfaces
            foreach (Surface s in c.Surfaces)
            {
                GetSurfaceVertices(s, t);
            }
            foreach (Group g in c.Groups)
            {
                DrawGroup(g, t);
            }

            // draw each sub instance
            foreach (Instance i in c.Instances)
            {
                DrawInstance(i, t);
            }
        }
        #endregion

        #region Helper functions
        private static void GetSurfaceVertices(Surface surface)
        {
            //faceStrings.Add("s");
            if (UsesTransparency(surface)) faceStrings.Add("t");
            Mesh m = surface.FaceMesh;

            foreach (MeshFace mf in m.Faces)
            {

                SetVertex(vertice, m.Vertices[mf.A]);
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, m.Vertices[mf.B]);
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, m.Vertices[mf.C]);
                faceStrings.Add(vertice.ToString());
            }
        }

        private static List<string> GetSurfaceVerticesList(Surface surface) { 
            Mesh m = surface.FaceMesh;
            List<string> currentSurface = new List<string>();
            foreach (MeshFace mf in m.Faces)
            {

                SetVertex(vertice, m.Vertices[mf.A]);
                currentSurface.Add(vertice.ToString());

                SetVertex(vertice, m.Vertices[mf.B]);
                currentSurface.Add(vertice.ToString());

                SetVertex(vertice, m.Vertices[mf.C]);
                currentSurface.Add(vertice.ToString());
            }
            return currentSurface;
        }

        private static List<string> GetSurfaceVerticesList(Surface surface, List<Transform> transforms)
        {
            Mesh m = surface.FaceMesh;
            List<string> currentSurface = new List<string>();
            foreach (MeshFace mf in m.Faces)
            {

                Vertex v1 = m.Vertices[mf.A]; ;
                for (int i = transforms.Count; i > 0; i--)
                {
                    v1 = transforms[i - 1].GetTransformed(v1);
                }
                SetVertex(vertice, v1);
                currentSurface.Add(vertice.ToString());

                v1 = m.Vertices[mf.B];
                for (int i = transforms.Count; i > 0; i--)
                {
                    v1 = transforms[i - 1].GetTransformed(v1);
                }
                SetVertex(vertice, v1);
                currentSurface.Add(vertice.ToString());

                v1 = m.Vertices[mf.C]; ;
                for (int i = transforms.Count; i > 0; i--)
                {
                    v1 = transforms[i - 1].GetTransformed(v1);
                }
                SetVertex(vertice, v1);
                currentSurface.Add(vertice.ToString());
            }
            return currentSurface;
        }

        private static void GetSurfaceVertices(Surface surface, List<Transform> transforms)
        {

            //faceStrings.Add("s");
            if (UsesTransparency(surface)) faceStrings.Add("t");
            Mesh m = surface.FaceMesh;

            foreach (MeshFace mf in m.Faces)
            {

                Vertex v1 = m.Vertices[mf.A]; ;
                for (int i = transforms.Count; i > 0; i--)
                {
                    v1 = transforms[i - 1].GetTransformed(v1);
                }
                SetVertex(vertice, v1);
                faceStrings.Add(vertice.ToString());

                v1 = m.Vertices[mf.B];
                for (int i = transforms.Count; i > 0; i--)
                {
                    v1 = transforms[i - 1].GetTransformed(v1);
                }
                SetVertex(vertice, v1);
                faceStrings.Add(vertice.ToString());

                v1 = m.Vertices[mf.C]; ;
                for (int i = transforms.Count; i > 0; i--)
                {
                    v1 = transforms[i - 1].GetTransformed(v1);
                }
                SetVertex(vertice, v1);
                faceStrings.Add(vertice.ToString());
            }
        }

        private static void SetVertex(Vector3 vertex, Vertex skpVertex)
        {
            vertex.x = skpVertex.X;
            vertex.y = skpVertex.Y;
            vertex.z = skpVertex.Z;
        }
        
        private static Colour GetSurfaceColour(Surface surface, bool isBackMaterial = false)
        {
            Material mat = new Material();
            mat = (isBackMaterial) ? surface.BackMaterial : surface.FrontMaterial;
            double R = mat.Colour.R;
            double G = mat.Colour.G;
            double B = mat.Colour.B;
            double A = mat.Colour.A;

            return new Colour(R, G, B, A);
        }

        private static void GetSurfaceBackMaterialColour(Surface surface)
        {
            Material mat = surface.BackMaterial;
            double R = mat.Colour.R;
            double G = mat.Colour.G;
            double B = mat.Colour.B;
            double A = mat.Colour.A;
            double O = mat.Opacity;

            faceStrings.Add(new Colour(R, G, B, A, O).ToString());
        }

        private static bool UsesTransparency(Surface s)
        {
            return s.FrontMaterial.UseOpacity;
        }

        #endregion

    }
    #region Helper classes
    public class Vector3
    {
        public double x;
        public double y;
        public double z;

        public Vector3(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3()
        {
        }

        public override string ToString()
        {
            return x + "p" + y + "p" + z;
        }

        public string ToTransformString()
        {
            return x + "s" + y + "s" + z;
        }
    }

    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }

    public class Colour
    {
        public double R, G, B, A, Opacity;
        public Colour() { }
        public Colour(double R, double G, double B, double A = 255, double Opacity = 1)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
            this.Opacity = Opacity;
        }

        public override string ToString()
        {
            return R + "c" + G + "c" + G + "c" + A + "c" + Opacity;
        }
    }
    #endregion

}
