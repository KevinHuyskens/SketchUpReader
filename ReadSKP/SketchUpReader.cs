using System;
using System.Collections.Generic;
using System.Threading;
using SketchUpNET;
using System.IO;
namespace ReadSKP
{
    class SketchUpReader
    {
        private Vector3 vertice = new Vector3();
        UdpClient client = new UdpClient();

        List<string> faceStrings = new List<string>();

        public int verticeCount = 0;

        public void loadSkp(string path)
        {
            var currentTime = DateTime.Now;
            SketchUp skp = new SketchUp();
            if (skp.LoadModel(path, true))
            {
                foreach(Instance i in skp.Instances)
                {
                    
                    foreach (KeyValuePair<string, SketchUpNET.Component> c in skp.Components)
                    {
                        DrawComponent(c.Value, new List<Transform>());
                        
                    }

                    foreach (Surface s in skp.Surfaces)
                    {
                        faceStrings.Add(s.BackMaterial.Colour.R + "c" + s.BackMaterial.Colour.G + "c" + s.BackMaterial.Colour.B + "c" + s.BackMaterial.Colour.A);
                        faceStrings.Add(s.FrontMaterial.Colour.R + "c" + s.FrontMaterial.Colour.G + "c" + s.FrontMaterial.Colour.B + "c" + s.FrontMaterial.Colour.A);
                        GetSurfaceVertices(s);
                    }

                    DrawInstance(i, new List<Transform>());
                }
                foreach (Group g in skp.Groups)
                {
                    DrawGroup(g, new List<Transform>());
                }
                
                if(skp.Instances.Count == 0)
                {
                    foreach (KeyValuePair<string, SketchUpNET.Component> c in skp.Components)
                    {
                        DrawComponent(c.Value, new List<Transform>());
                    }
                }

                foreach (Surface s in skp.Surfaces)
                {
                    faceStrings.Add(s.BackMaterial.Colour.R + "c" + s.BackMaterial.Colour.G + "c" + s.BackMaterial.Colour.B + "c" + s.BackMaterial.Colour.A);
                    faceStrings.Add(s.FrontMaterial.Colour.R + "c" + s.FrontMaterial.Colour.G + "c" + s.FrontMaterial.Colour.B + "c" + s.FrontMaterial.Colour.A);
                    faceStrings.Add(s.BackMaterial.MaterialTexture.Name);
                    faceStrings.Add(s.FrontMaterial.MaterialTexture.Name);
                    GetSurfaceVertices(s);
                }


                File.WriteAllLines(@"c:\Users\Kevin\Desktop\testfile.txt", faceStrings);
                
                
            }
        }


        private void DrawGroup(Group g, List<Transform> t)
        {
            faceStrings.Add("s");
            t.Add(g.Transformation);
            // draw instances 
            foreach (Instance i in g.Instances)
            {
                DrawInstance(i, t);
            }
            // draw groups
            foreach(Group childG in g.Groups)
            {
                DrawGroup(childG, t);
            }

            // done clear trans
            t.RemoveAt(t.Count - 1);
        }

        private void DrawInstance( Instance i, List<Transform> t)
        {
            faceStrings.Add("s");
            t.Add(i.Transformation);

            // draw instance 'mesh

            if(i.Parent == null)
            {
                t.RemoveAt(t.Count - 1);
                return;
            }
            DrawComponent(i.Parent as Component, t);

            // done clear trans
            t.RemoveAt(t.Count - 1);
        }

        private void DrawComponent(Component c, List<Transform> t)
        {
            faceStrings.Add("s");
            // draw surfaces
            foreach (Surface s in c.Surfaces)
            {
                faceStrings.Add(s.BackMaterial.Colour.R + "c" + s.BackMaterial.Colour.G + "c" + s.BackMaterial.Colour.B + "c" + s.BackMaterial.Colour.A);
                faceStrings.Add(s.FrontMaterial.Colour.R + "c" + s.FrontMaterial.Colour.G + "c" + s.FrontMaterial.Colour.B + "c" + s.FrontMaterial.Colour.A);
                faceStrings.Add(s.BackMaterial.MaterialTexture.Name);
                faceStrings.Add(s.FrontMaterial.MaterialTexture.Name);
                GetSurfaceVertices(s, t);
            }

            foreach(Group g in c.Groups)
            {
                DrawGroup(g, t);
            }

            // draw each sub instance
            foreach(Instance i in c.Instances)
            {
                DrawInstance(i, t);
            }
        }


        private void GetSurfaceVertices(Surface surface)
        {

            Mesh m = surface.FaceMesh;

            foreach(MeshFace mf in m.Faces)
            {
                verticeCount += 3;

                SetVertex(vertice, m.Vertices[mf.A]);
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, m.Vertices[mf.B]);
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, m.Vertices[mf.C]);
                faceStrings.Add(vertice.ToString());
            }
        }

        private void GetSurfaceVertices(Surface surface, Instance i)
        {
            Mesh m = surface.FaceMesh;

            foreach (MeshFace mf in m.Faces)
            {
                verticeCount += 3;

                SetVertex(vertice, i.Transformation.GetTransformed(m.Vertices[mf.A]));
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, i.Transformation.GetTransformed(m.Vertices[mf.B]));
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, i.Transformation.GetTransformed(m.Vertices[mf.C]));
                faceStrings.Add(vertice.ToString());
            }
        }

        private void GetSurfaceVertices(Surface surface, List<Transform> transforms)
        {
            Mesh m = surface.FaceMesh;

            foreach (MeshFace mf in m.Faces)
            {
                verticeCount += 3;

                Vertex v1 = m.Vertices[mf.A]; ;
                for(int i = transforms.Count; i>0; i--)
                {
                    v1 = transforms[i-1].GetTransformed(v1);
                }
                SetVertex(vertice,v1);
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
        private void GetSurfaceVertices(Surface surface, Group g)
        {
            Mesh m = surface.FaceMesh;

            foreach (MeshFace mf in m.Faces)
            {
                verticeCount += 3;

                SetVertex(vertice, g.Transformation.GetTransformed(m.Vertices[mf.A]));
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, g.Transformation.GetTransformed(m.Vertices[mf.B]));
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, g.Transformation.GetTransformed(m.Vertices[mf.C]));
                faceStrings.Add(vertice.ToString());
            }
        }

        private void GetSurfaceVertices(Surface surface, Group g, Instance i)
        {
            Mesh m = surface.FaceMesh;

            foreach (MeshFace mf in m.Faces)
            {
                verticeCount += 3;

                SetVertex(vertice, g.Transformation.GetTransformed(i.Transformation.GetTransformed(m.Vertices[mf.A])));
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, g.Transformation.GetTransformed(i.Transformation.GetTransformed(m.Vertices[mf.B])));
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, g.Transformation.GetTransformed(i.Transformation.GetTransformed(m.Vertices[mf.C])));
                faceStrings.Add(vertice.ToString());
            }
        }

        private void GetSurfaceVertices(Surface surface, Instance rootInstance,Instance nestedInstance)
        {
            Mesh m = surface.FaceMesh;

            foreach (MeshFace mf in m.Faces)
            {
                verticeCount += 3;

                SetVertex(vertice, rootInstance.Transformation.GetTransformed(nestedInstance.Transformation.GetTransformed(m.Vertices[mf.A])));
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, rootInstance.Transformation.GetTransformed(nestedInstance.Transformation.GetTransformed(m.Vertices[mf.B])));
                faceStrings.Add(vertice.ToString());

                SetVertex(vertice, rootInstance.Transformation.GetTransformed(nestedInstance.Transformation.GetTransformed(m.Vertices[mf.C])));
                faceStrings.Add(vertice.ToString());
            }
        }

        private void SetVertex(Vector3 vertex, Vertex skpVertex)
        {
            vertex.x = skpVertex.X;
            vertex.y = skpVertex.Y;
            vertex.z = skpVertex.Z;
        }

        public static bool ReformatModel(string filepath, string version, string newfilepath)
        {
            SketchUp skp = new SketchUp();
            SKPVersion v = SKPVersion.V2017;
            switch (version)
            {
                case "2014": v = SKPVersion.V2014; break;
                case "2015": v = SKPVersion.V2015; break;
                case "2016": v = SKPVersion.V2016; break;
                case "2017": v = SKPVersion.V2017; break;
                case "2018": v = SKPVersion.V2018; break;
            }
            return skp.SaveAs(filepath, v, newfilepath);
        }
    }

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
}

