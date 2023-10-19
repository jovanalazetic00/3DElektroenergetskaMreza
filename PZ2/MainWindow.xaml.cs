using PZ2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

namespace PZ2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<SubstationEntity> substations;
        List<NodeEntity> nodes;
        List<SwitchEntity> switches;
        List<LineEntity> lines;
        Dictionary<long, PowerEntity> all_entities = new Dictionary<long, PowerEntity>();
        Dictionary<GeometryModel3D, Material> coloredEntites = new Dictionary<GeometryModel3D, Material>();
        Dictionary<long, int> relatedEntity = new Dictionary<long, int>();

        private Point start = new Point();
        private Point diffOffset = new Point();
        private Point rotationStart = new Point();
        private int maxZoom = 70;
        private int trenutniZoom = 1;

        double dlx = 45.2325;
        double dly = 19.793909;
        double gdx = 45.277031;
        double gdy = 19.894459;
        int map_zone = 34;

        PerspectiveCamera pc = new PerspectiveCamera();
        DirectionalLight dlight = new DirectionalLight();

        List<Tuple<string, string>> on_map_el = new List<Tuple<string, string>>();

        List<Tuple<long, GeometryModel3D>> drawn_objects = new List<Tuple<long, GeometryModel3D>>();

        private GeometryModel3D hitgeo;

        List<long> cube_ids_on_map = new List<long>();

        public bool SwitchesAreColored { get; set; } = true;
        public bool CheckedAllCon { get; set; }
        public bool Checked { get; set; }
        public bool Checked35 { get; set; }
        public bool Checked03 { get; set; }
        public bool CheckedOpen { get; set; }
        public bool CheckedR01color { get; set; }
        public bool CheckedR12color { get; set; }
        public bool CheckedR3color { get; set; }
        public bool CheckedRevertLineColor { get; set; }
        public bool CheckedSwitchStatus { get; set; }
        public bool RevertSwitchStatus { get; set; }
      

        public MainWindow()
        {
            InitializeComponent();
            cbOpenShow.DataContext = this;
            cbOpenHide.DataContext = this;
            CheckedR01color = false;
            CheckedR12color = false;
            CheckedR3color = false;
            CheckedRevertLineColor = false;
            CheckedOpen = false;
            Checked03 = false;
            Checked35 = false;
            Checked = false;
            CheckedAllCon = true;
            CheckedSwitchStatus = false;
            RevertSwitchStatus = false;
            
            Get_from_file();
            viewPort.Camera = null;
            pc.Position = new Point3D(0, 0, 2);
            pc.LookDirection = new Vector3D(0, 0, -1);
            pc.UpDirection = new Vector3D(0, 1, 0);
            pc.FieldOfView = 90;
            viewPort.Camera = pc;

            dlight.Color = Colors.White;
            dlight.Direction = new Vector3D(2, -9, -1);

            double latitude = 0;
            double longitude = 0;

            foreach (SubstationEntity substation in substations)
            {
                ToLatLon(substation.X, substation.Y, map_zone, out latitude, out longitude);

                string prvi = latitude.ToString().Substring(0, 6);
                string drugi = longitude.ToString().Substring(0, 6);
                Console.WriteLine(prvi + "   " + drugi);
                Tuple<string, string> element = null;
                if (on_map_el.Count != 0)
                {
                    element = on_map_el.Find(el => el.Item1.Contains(prvi) && el.Item2.Contains(drugi));
                }

                on_map_el.Add(new Tuple<string, string>(latitude.ToString(), longitude.ToString()));
                Console.WriteLine("original");
                double z1 = 0.0;
                double z2 = 0.01;

                //broj konekcija
                int num_connections = 0;
                List<LineEntity> entities_connected = lines.FindAll(xx => xx.FirstEnd == substation.Id || xx.SecondEnd == substation.Id);
                num_connections = entities_connected.Count();

                GeometryModel3D substation_cube = Draw_cube(latitude, longitude, substation.Id, "substation", z1, z2, num_connections);
                if (substation_cube != null)
                {
                    MyView.Children.Add(substation_cube); 

                    drawn_objects.Add(new Tuple<long, GeometryModel3D>(substation.Id, substation_cube));
                    cube_ids_on_map.Add(substation.Id);
                    substation.Num_connections = num_connections;
                    all_entities.Add(substation.Id, substation);
                }

            }
            int br_sw = 0;
            DrawSwithces(latitude, longitude, ref br_sw);

            int br_nodova = 0;
            foreach (NodeEntity node in nodes)
            {
                ToLatLon(node.X, node.Y, map_zone, out latitude, out longitude);
                double z1 = 0.0;
                double z2 = 0.01;

                string prvi = latitude.ToString().Substring(0, 6);      
                string drugi = longitude.ToString().Substring(0, 6);    
                List<Tuple<string, string>> element = new List<Tuple<string, string>>();
                if (on_map_el.Count != 0)
                {
                    element = on_map_el.FindAll(el => el.Item1.Contains(prvi) && el.Item2.Contains(drugi));
                    if (element.Count != 0)
                    {
                        foreach (var el in element)
                        {
                            z1 = z1 + 0.01;
                            z2 = z2 + 0.01;
                        }
                    }
                }

                on_map_el.Add(new Tuple<string, string>(latitude.ToString(), longitude.ToString()));

                int num_connections = 0;
                List<LineEntity> entities_connected = lines.FindAll(xx => xx.FirstEnd == node.Id || xx.SecondEnd == node.Id);
                num_connections = entities_connected.Count();
                Console.WriteLine("VEZA: " + num_connections);

                GeometryModel3D node_cube = Draw_cube(latitude, longitude, node.Id, "node", z1, z2, num_connections);
                if (node_cube != null)
                {
                    MyView.Children.Add(node_cube);
                    drawn_objects.Add(new Tuple<long, GeometryModel3D>(node.Id, node_cube));
                    cube_ids_on_map.Add(node.Id);
                    node.Num_connections = num_connections;
                    all_entities.Add(node.Id, node);
                }
            }

            DrawLines();

            MyView.Children.Add(new AmbientLight(Colors.Gray));
            Console.WriteLine("0000333 --- " + Checked03);
        }

        private void DrawLines()
        {
            double x1, y1, x2, y2;
            int aaa = 0;
            foreach (LineEntity l in lines)
            {
                if (cube_ids_on_map.Contains(l.FirstEnd) && cube_ids_on_map.Contains(l.SecondEnd))
                {
                    aaa++;
                    SolidColorBrush boja = new SolidColorBrush(Colors.Purple);
                    if (l.ConductorMaterial.Equals("Steel"))
                    {
                        boja = new SolidColorBrush(Colors.Gray);
                    }
                    else if (l.ConductorMaterial.Equals("Copper"))
                    {
                        boja = new SolidColorBrush(Colors.Orange);
                    }
                    else if (l.ConductorMaterial.Equals("Acsr"))
                    {
                        boja = new SolidColorBrush(Colors.DarkRed);
                    }
                    else if (l.ConductorMaterial.Equals("Other"))
                    {
                        boja = new SolidColorBrush(Colors.Aqua);
                    }

                    for (int i = 0; i < l.Vertices.Count; i++)
                    {
                        if (i != l.Vertices.Count - 1)
                        {
                            ToLatLon(l.Vertices[i].X, l.Vertices[i].Y, map_zone, out x1, out y1);

                            ToLatLon(l.Vertices[i + 1].X, l.Vertices[i + 1].Y, map_zone, out x2, out y2);
                            GeometryModel3D lineObj = this.Draw_line(x1, y1, x2, y2, boja);

                            MyView.Children.Add(lineObj);
                            drawn_objects.Add(new Tuple<long, GeometryModel3D>(l.Id, lineObj));
                            cube_ids_on_map.Add(l.Id);
                        }
                    }
                }
            }

        }

        private void DrawSwithces(double latitude, double longitude, ref int br_sw)
        {
            foreach (SwitchEntity sw in switches)
            {
                ToLatLon(sw.X, sw.Y, map_zone, out latitude, out longitude);

                double z1 = 0.0;
                double z2 = 0.01;

                string prvi = latitude.ToString().Substring(0, 6);      
                string drugi = longitude.ToString().Substring(0, 6);    
                Console.WriteLine(prvi + "   " + drugi);
                List<Tuple<string, string>> element = new List<Tuple<string, string>>();
                if (on_map_el.Count != 0) 
                {
                    element = on_map_el.FindAll(el => el.Item1.Contains(prvi) && el.Item2.Contains(drugi));
                    if (element.Count == 0)
                    {
                        Console.WriteLine("ORG");

                    }
                    else
                    {
                        Console.WriteLine("br pon:" + element.Count());
                        Console.WriteLine("====");

                        foreach (var el in element)                                                         // za svaki na istom mjestu povecavam visinu
                        {
                            Console.WriteLine($"\t{el.Item1}   {el.Item2}");
                            z1 = z1 + 0.01;
                            z2 = z2 + 0.01;
                        }
                        Console.WriteLine($"UVECANO {z1}  {z2}");
                        Console.WriteLine("====");
                        Console.WriteLine("ponavljaju se-----------------");
                    }
                }

                on_map_el.Add(new Tuple<string, string>(latitude.ToString(), longitude.ToString()));
                Console.WriteLine("original");

                int num_connections = 0;
                List<LineEntity> entities_connected = lines.FindAll(xx => xx.FirstEnd == sw.Id || xx.SecondEnd == sw.Id);
                num_connections = entities_connected.Count();

                Console.WriteLine("VEZA: " + num_connections);

                GeometryModel3D switch_cube = Draw_cube(latitude, longitude, sw.Id, "switch", z1, z2, num_connections);              //CRTANJE
                if (switch_cube != null)
                {
                    
                    {
                        Console.WriteLine($"{br_sw++}_SW  lat_ {latitude}----lon_  {longitude}");
                        MyView.Children.Add(switch_cube);

                        drawn_objects.Add(new Tuple<long, GeometryModel3D>(sw.Id, switch_cube));
                        cube_ids_on_map.Add(sw.Id);
                        sw.Num_connections = num_connections;
                        all_entities.Add(sw.Id, sw);

                    }
                }
            }

        }

        public GeometryModel3D Draw_line(double x1, double y1, double x2, double y2, SolidColorBrush boja)
        {
            double centarX = gdx - dlx;
            double centarY = gdy - dly;

            double pozicijaX1 = Math.Abs(dlx - x1);
            double pozicijaY1 = Math.Abs(dly - y1);

            double pozicijaX2 = Math.Abs(dlx - x2);
            double pozicijaY2 = Math.Abs(dly - y2);

            var materijal = new DiffuseMaterial(boja);
            var mesh = new MeshGeometry3D();
            var lineSIZE = 0.004;

            Point3D teme1 = new Point3D(-1 + 2 * pozicijaY1 / centarY, -1 + 2 * pozicijaX1 / centarX, 0.01);
            Point3D teme2 = new Point3D(-0.999 + 2 * pozicijaY1 / centarY + 0.004, -1 + 2 * pozicijaX1 / centarX, 0.01);
            Point3D teme3 = new Point3D(-1 + 2 * pozicijaY2 / centarY, -1 + 2 * pozicijaX2 / centarX, 0.01);

            Point3D teme4 = new Point3D(-0.999 + 2 * pozicijaY2 / centarY + 0.004, -1 + 2 * pozicijaX2 / centarX, 0.01);
            Point3D teme5 = new Point3D(-1 + 2 * pozicijaY1 / centarY + 0.002, -0.999 + 2 * pozicijaX1 / centarX, 0.01 + lineSIZE);
            Point3D teme6 = new Point3D(-0.999 + 2 * pozicijaY2 / centarY + 0.002, -0.999 + 2 * pozicijaX2 / centarX, 0.01 + lineSIZE);

            mesh.Positions.Add(teme1);
            mesh.Positions.Add(teme2);
            mesh.Positions.Add(teme3);
            mesh.Positions.Add(teme4);
            mesh.Positions.Add(teme5);
            mesh.Positions.Add(teme6);

            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(4);

            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(3);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(1);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(0);
            //
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(2);

            mesh.TriangleIndices.Add(2);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);
            mesh.TriangleIndices.Add(3);

            mesh.TriangleIndices.Add(4);
            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(5);

            mesh.TriangleIndices.Add(1);
            mesh.TriangleIndices.Add(0);
            mesh.TriangleIndices.Add(4);

            return new GeometryModel3D(mesh, materijal);
        }
        private void Get_from_file()
        {
            XDocument fajl = XDocument.Load("Geographic.xml");
            var networkModel = fajl.Descendants("NetworkModel");

            var subList = networkModel.Descendants("Substations").Descendants("SubstationEntity")
                .Select(x => new SubstationEntity
                {
                    Id = long.Parse(x.Element("Id").Value),
                    Name = x.Element("Name").Value,
                    X = double.Parse(x.Element("X").Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo),
                    Y = double.Parse(x.Element("Y").Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo)
                }).ToList();

            substations = subList;

            var nodeList = networkModel.Descendants("Nodes").Descendants("NodeEntity")
                .Select(x => new NodeEntity
                {
                    Id = long.Parse(x.Element("Id").Value),
                    Name = x.Element("Name").Value,
                    X = double.Parse(x.Element("X").Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo),
                    Y = double.Parse(x.Element("Y").Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo)
                }).ToList();

            nodes = nodeList;

            var switcheList = networkModel.Descendants("Switches").Descendants("SwitchEntity")
                .Select(x => new SwitchEntity
                {
                    Id = long.Parse(x.Element("Id").Value),
                    Name = x.Element("Name").Value,
                    Status = x.Element("Status").Value,
                    X = double.Parse(x.Element("X").Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo),
                    Y = double.Parse(x.Element("Y").Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo)
                }).ToList();

            switches = switcheList;

            var lineList = networkModel.Descendants("Lines").Descendants("LineEntity")
                .Select(x => new LineEntity
                {
                    Id = long.Parse(x.Element("Id").Value),
                    Name = x.Element("Name").Value,
                    IsUnderground = bool.Parse(x.Element("IsUnderground").Value),
                    R = float.Parse(x.Element("R").Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo),
                    ConductorMaterial = x.Element("ConductorMaterial").Value,
                    LineType = x.Element("LineType").Value,
                    ThermalConstantHeat = Int32.Parse(x.Element("ThermalConstantHeat").Value),
                    FirstEnd = long.Parse(x.Element("FirstEnd").Value),
                    SecondEnd = long.Parse(x.Element("SecondEnd").Value),
                    Vertices = x.Elements("Vertices").Descendants("Point").Select(y => new PointElement
                    {
                        X = double.Parse(y.Element("X").Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo),
                        Y = double.Parse(y.Element("Y").Value, System.Globalization.NumberStyles.AllowDecimalPoint, System.Globalization.NumberFormatInfo.InvariantInfo)
                    }).ToList()
                }).ToList();

            lines = lineList;
           
        }
        public GeometryModel3D Draw_cube(double pozicijaX, double pozicijaY, long id, string tip, double tempZ1, double tempZ2, int num_connections)
        {
            if ((pozicijaX > gdx || pozicijaX < dlx) || (pozicijaY > gdy || pozicijaY < dly))
            {
                return null;
            }
            else
            {
                double centarX = gdx - dlx;
                double centarY = gdy - dly;
                double z1 = 0;
                double z2 = 0;

                pozicijaX = Math.Abs(dlx - pozicijaX);
                pozicijaY = Math.Abs(dly - pozicijaY);
                SolidColorBrush brush = new SolidColorBrush();

                if (tip.Equals("substation"))
                {
                    z1 = 0.0;
                    z2 = 0.01;
                    brush = new SolidColorBrush(Colors.Magenta);
                }
                else if (tip.Equals("switch"))
                {
                    z1 = tempZ1;//0;
                    z2 = tempZ2;//0.01;
                    brush = new SolidColorBrush(Colors.Purple);
                }
                else if (tip.Equals("node"))
                {
                    z1 = tempZ1;//0.01;
                    z2 = tempZ2;//0.02;
                    brush = new SolidColorBrush(Colors.DarkBlue);
                }


                var materijal = new DiffuseMaterial(brush);
                var mesh = new MeshGeometry3D();

                Point3D teme1 = new Point3D(-1 + 2 * pozicijaY / centarY, -1 + 2 * pozicijaX / centarX, z1);
                Point3D teme2 = new Point3D(-0.99 + 2 * pozicijaY / centarY, -1 + 2 * pozicijaX / centarX, z1);
                Point3D teme3 = new Point3D(-1 + 2 * pozicijaY / centarY, -0.99 + 2 * pozicijaX / centarX, z1);
                Point3D teme4 = new Point3D(-0.99 + 2 * pozicijaY / centarY, -0.99 + 2 * pozicijaX / centarX, z1);

                Point3D teme5 = new Point3D(-1 + 2 * pozicijaY / centarY, -1 + 2 * pozicijaX / centarX, z2);
                Point3D teme6 = new Point3D(-0.99 + 2 * pozicijaY / centarY, -1 + 2 * pozicijaX / centarX, z2);
                Point3D teme7 = new Point3D(-1 + 2 * pozicijaY / centarY, -0.99 + 2 * pozicijaX / centarX, z2);
                Point3D teme8 = new Point3D(-0.99 + 2 * pozicijaY / centarY, -0.99 + 2 * pozicijaX / centarX, z2);


                mesh.Positions.Add(teme1);
                mesh.Positions.Add(teme2);
                mesh.Positions.Add(teme3);
                mesh.Positions.Add(teme4);
                mesh.Positions.Add(teme5);
                mesh.Positions.Add(teme6);
                mesh.Positions.Add(teme7);
                mesh.Positions.Add(teme8);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(1);

                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(1);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(4);
                mesh.TriangleIndices.Add(5);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(0);

                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(4);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(3);

                mesh.TriangleIndices.Add(2);
                mesh.TriangleIndices.Add(6);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(1);
                mesh.TriangleIndices.Add(5);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(5);
                mesh.TriangleIndices.Add(4);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(3);
                mesh.TriangleIndices.Add(7);

                mesh.TriangleIndices.Add(0);
                mesh.TriangleIndices.Add(7);
                mesh.TriangleIndices.Add(4);

                return new GeometryModel3D(mesh, materijal);
            }
        }

        private void view3D_Hit(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point mousePosition = e.GetPosition(viewPort);
            Point3D testPoint = new Point3D(mousePosition.X, mousePosition.Y, 0);
            Vector3D testDirection = new Vector3D(mousePosition.X, mousePosition.Y, 10);

            PointHitTestParameters pointParams =
                     new PointHitTestParameters(mousePosition);
            RayHitTestParameters rayParams =
                     new RayHitTestParameters(testPoint, testDirection);

            hitgeo = null;
            VisualTreeHelper.HitTest(viewPort, null, HTResult, pointParams);
        }

        private HitTestResultBehavior HTResult(System.Windows.Media.HitTestResult rawresult)
        {
            RayHitTestResult rayResult = rawresult as RayHitTestResult;

            if (rayResult != null)
            {
                bool gasit = false;
                for (int i = 0; i < drawn_objects.Count; i++)
                {
                    if ((GeometryModel3D)drawn_objects[i].Item2 == rayResult.ModelHit)      
                    {
                        hitgeo = (GeometryModel3D)rayResult.ModelHit;
                        ToolTip toolTip = new ToolTip();
                        gasit = true;
                        foreach (SubstationEntity s in substations)
                        {
                            if (s.Id == cube_ids_on_map[i])
                            {
                                toolTip.Content = "Substation\n\tID: " + s.Id + "\n\tName: " + s.Name;
                                toolTip.StaysOpen = false;
                                toolTip.IsOpen = true;
                                
                            }
                        }
                        foreach (NodeEntity n in nodes)
                        {
                            if (n.Id == cube_ids_on_map[i])
                            {
                                toolTip.Content = "Node\n\tID: " + n.Id + "\n\tName: " + n.Name;
                                toolTip.StaysOpen = false;
                                toolTip.IsOpen = true;
                            }
                        }
                        foreach (SwitchEntity sw in switches)
                        {
                            if (sw.Id == cube_ids_on_map[i])
                            {
                                toolTip.Content = "Switch\n\tID: " + sw.Id + "\n\tName: " + sw.Name;
                                toolTip.StaysOpen = false;
                                toolTip.IsOpen = true;
                            }
                        }
                        foreach (LineEntity line in lines)
                        {

                            if (line.Id == cube_ids_on_map[i])
                            {
                                foreach(var geometryModel3D in coloredEntites)
                                {
                                    geometryModel3D.Key.Material = geometryModel3D.Value;
                                }
                                coloredEntites.Clear();
                                toolTip.Content = "Line\n\tID: " + line.Id + "\n\tName: " + line.Name;
                                toolTip.StaysOpen = false;
                                toolTip.IsOpen = true;

                                var cube1 = drawn_objects.Find(xx => xx.Item1 == line.FirstEnd).Item2;       
                                var cube2 = drawn_objects.Find(xx => xx.Item1 == line.SecondEnd).Item2;

                                SolidColorBrush brush2 = new SolidColorBrush(Colors.Green);

                                var materija2 = new DiffuseMaterial(brush2);
                                Console.WriteLine(cube1.Material.ToString());
                                Console.WriteLine(materija2.ToString());
                                if (cube1.Material.ToString().Equals(materija2.ToString()) || cube2.Material.ToString().Equals(materija2.ToString())) { Console.WriteLine("TRANPARENTNO 0000"); }
                                SolidColorBrush brush = new SolidColorBrush(Colors.Green);

                                var materijal = new DiffuseMaterial(brush);
                                coloredEntites.Add(cube1,cube1.Material);
                                coloredEntites.Add(cube2,cube2.Material);
                                cube1.Material = materijal;
                                cube2.Material = materijal;
                            }
                        }
                    }
                }
                if (!gasit)
                {
                    hitgeo = null;
                }
            }

            return HitTestResultBehavior.Stop;
        }
        private void cbswitchcolor_Checked(object sender, RoutedEventArgs e)
        {
            drawn_objects.Where(x => switches.Any(s => s.Id == x.Item1 && s.Status == "Open")).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Green)));
            drawn_objects.Where(x => switches.Any(s => s.Id == x.Item1 && s.Status == "Closed")).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red)));
        }
        private void cbrevertswitch_Checked(object sender, RoutedEventArgs e) =>
            drawn_objects.Where(x => switches.Any(s => s.Id == x.Item1)).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Purple)));


        

        private void cbR01color_Checked(object sender, RoutedEventArgs e) =>
            drawn_objects.Where(x => lines.Any(l => l.Id == x.Item1 && l.R < 1)).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Red)));
        private void cbR12color_Checked(object sender, RoutedEventArgs e) =>
            drawn_objects.Where(x => lines.Any(l => l.Id == x.Item1 && l.R >= 1 && l.R <= 2)).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Orange)));
        private void cbR3color_Checked(object sender, RoutedEventArgs e) =>
            drawn_objects.Where(x => lines.Any(l => l.Id == x.Item1 && l.R > 2)).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Yellow)));

        private void ColorLines()
        {
            drawn_objects.Where(x => lines.Any(l => l.Id == x.Item1 && l.ConductorMaterial == "Copper")).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Orange)));
            drawn_objects.Where(x => lines.Any(l => l.Id == x.Item1 && l.ConductorMaterial == "Steel")).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray)));
            drawn_objects.Where(x => lines.Any(l => l.Id == x.Item1 && l.ConductorMaterial == "Acsr")).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.DarkRed)));
            drawn_objects.Where(x => lines.Any(l => l.Id == x.Item1 && l.ConductorMaterial == "Other")).ToList().ForEach(x => x.Item2.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Aqua)));
        }
        private void cbRevertLineColor_Checked(object sender, RoutedEventArgs e)
        {
            ColorLines();
        }

        private void Move_map(object sender, MouseButtonEventArgs e)
        {
            viewPort.CaptureMouse();
            start = e.GetPosition(this);
            diffOffset.X = transliranje.OffsetX;
            diffOffset.Y = transliranje.OffsetY;

            Point pozicijaMisa = e.GetPosition(viewPort);
            Point3D point3D = new Point3D(pozicijaMisa.X, pozicijaMisa.Y, 0);
            Vector3D pravac = new Vector3D(pozicijaMisa.X, pozicijaMisa.Y, 10);
        }

        private void otpustanjeTastera(object sender, MouseButtonEventArgs e)
        {
            viewPort.ReleaseMouseCapture();
        }

        private void ZaRotiranje(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
                rotationStart = e.GetPosition(this);
        }

        private void rotacijaMape(object sender, MouseEventArgs e)
        {

            if (viewPort.IsMouseCaptured)
            {
                Point end = e.GetPosition(this);
                double offsetX = end.X - start.X;
                double offsetY = end.Y - start.Y;
                double w = this.Width;
                double h = this.Height;
                double translateX = (offsetX * 100) / w;
                double translateY = -(offsetY * 100) / h;
                transliranje.OffsetX = diffOffset.X + (translateX / (100 * skaliranje.ScaleX));
                transliranje.OffsetY = diffOffset.Y + (translateY / (100 * skaliranje.ScaleX));
            }
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                    Point start = e.GetPosition(this);

                    Vector delta = start - rotationStart;
                    myAngleRotationX.Angle -= delta.Y;
                    myAngleRotationY.Angle -= delta.X;

            }
            rotationStart = e.GetPosition(this);
        }

        private void Zoom_map(object sender, MouseWheelEventArgs e)
        {
            Point p = e.MouseDevice.GetPosition(this);
            if (e.Delta < 0 && trenutniZoom > 0)
            {
                pc.FieldOfView += 1;
                viewPort.Camera = pc;
                trenutniZoom--;
            }
            else if (e.Delta > 0 && trenutniZoom < maxZoom)
            {
                pc.FieldOfView -= 1;
                viewPort.Camera = pc;
                trenutniZoom++;
            }
        }

        public static void ToLatLon(double utmX, double utmY, int zoneUTM, out double latitude, out double longitude)
        {
            bool isNorthHemisphere = true;

            var diflat = -0.00066286966871111111111111111111111111;
            var diflon = -0.0003868060578;

            var zone = zoneUTM;
            var c_sa = 6378137.000000;
            var c_sb = 6356752.314245;
            var e2 = Math.Pow((Math.Pow(c_sa, 2) - Math.Pow(c_sb, 2)), 0.5) / c_sb;
            var e2cuadrada = Math.Pow(e2, 2);
            var c = Math.Pow(c_sa, 2) / c_sb;
            var x = utmX - 500000;
            var y = isNorthHemisphere ? utmY : utmY - 10000000;

            var s = ((zone * 6.0) - 183.0);
            var lat = y / (c_sa * 0.9996);
            var v = (c / Math.Pow(1 + (e2cuadrada * Math.Pow(Math.Cos(lat), 2)), 0.5)) * 0.9996;
            var a = x / v;
            var a1 = Math.Sin(2 * lat);
            var a2 = a1 * Math.Pow((Math.Cos(lat)), 2);
            var j2 = lat + (a1 / 2.0);
            var j4 = ((3 * j2) + a2) / 4.0;
            var j6 = ((5 * j4) + Math.Pow(a2 * (Math.Cos(lat)), 2)) / 3.0;
            var alfa = (3.0 / 4.0) * e2cuadrada;
            var beta = (5.0 / 3.0) * Math.Pow(alfa, 2);
            var gama = (35.0 / 27.0) * Math.Pow(alfa, 3);
            var bm = 0.9996 * c * (lat - alfa * j2 + beta * j4 - gama * j6);
            var b = (y - bm) / v;
            var epsi = ((e2cuadrada * Math.Pow(a, 2)) / 2.0) * Math.Pow((Math.Cos(lat)), 2);
            var eps = a * (1 - (epsi / 3.0));
            var nab = (b * (1 - epsi)) + lat;
            var senoheps = (Math.Exp(eps) - Math.Exp(-eps)) / 2.0;
            var delt = Math.Atan(senoheps / (Math.Cos(nab)));
            var tao = Math.Atan(Math.Cos(delt) * Math.Tan(nab));

            longitude = ((delt * (180.0 / Math.PI)) + s) + diflon;
            latitude = ((lat + (1 + e2cuadrada * Math.Pow(Math.Cos(lat), 2) - (3.0 / 2.0) * e2cuadrada * Math.Sin(lat) * Math.Cos(lat) * (tao - lat)) * (tao - lat)) * (180.0 / Math.PI)) + diflat;
        }

        private void cbOpen_UncheckedShow(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < drawn_objects.Count; i++)
            {
                foreach (LineEntity line in lines)
                {
                    if (line.Id == cube_ids_on_map[i])
                    {
                       

                        SwitchEntity prvi = switches.FirstOrDefault(x => x.Id == line.FirstEnd);
                        SwitchEntity drugi = switches.FirstOrDefault(x => x.Id == line.SecondEnd);

                        

                        if (prvi != null && drugi != null && (prvi.Status.ToLower().Equals("open") || drugi.Status.ToLower().Equals("open")))
                        {
                            MyView.Children.Add(drawn_objects.FirstOrDefault(x => x.Item1 == line.SecondEnd).Item2);
                            var cube = drawn_objects[i].Item2;
                            MyView.Children.Add(cube);
                        }
                    }
                }
            }
        }

        private void cbOpen_UncheckedHide(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < drawn_objects.Count; i++)
            {
                foreach (LineEntity line in lines)
                {
                    if (line.Id == cube_ids_on_map[i])
                    {

                        SwitchEntity prvi = switches.FirstOrDefault(x => x.Id == line.FirstEnd);
                        SwitchEntity drugi = switches.FirstOrDefault(x => x.Id == line.SecondEnd);

                       
                        if (prvi != null && drugi != null && (prvi.Status.ToLower().Equals("open") || drugi.Status.ToLower().Equals("open")))
                        {
                            MyView.Children.Remove(drawn_objects.FirstOrDefault(x => x.Item1 == line.SecondEnd).Item2);
                            var cube = drawn_objects[i].Item2;
                            MyView.Children.Remove(cube);
                        }
                    }
                }
            }
        }

       
       
    }
}
