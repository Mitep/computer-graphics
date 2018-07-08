// -----------------------------------------------------------------------
// <file>World.cs</file>
// <copyright>Grupa za Grafiku, Interakciju i Multimediju 2013.</copyright>
// <author>Srđan Mihić</author>
// <author>Aleksandar Josić</author>
// <summary>Klasa koja enkapsulira OpenGL programski kod.</summary>
// -----------------------------------------------------------------------
using System;
using Assimp;
using System.IO;
using System.Reflection;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Primitives;
using SharpGL.SceneGraph.Quadrics;
using SharpGL.SceneGraph.Core;
using SharpGL;
using SharpGL.Enumerations;
using System.Windows.Threading;
using System.Drawing;
using System.Drawing.Imaging;

namespace AssimpSample
{


    /// <summary>
    ///  Klasa enkapsulira OpenGL kod i omogucava njegovo iscrtavanje i azuriranje.
    /// </summary>
    public class World : IDisposable
    {
        #region Atributi

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        private AssimpScene m_scene;
        private AssimpScene m_scene2;

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        private float m_xRotation = 0.0f;

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        private float m_yRotation = 0.0f;

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        private float m_sceneDistance = 0.0f;

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_width;

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        private int m_height;

        private float[] lampLightPos = new float[] { 20.0f, 28.0f, -80.0f };
        private Cube cube;
        private Cylinder cylinder;
        private double cylinderFactor = 1;
        private double cubeFactor = 1;
        //animacija
        private DispatcherTimer timer1;
        private DispatcherTimer timer2;
        private bool animation = false;
        private float[] pozicijaKamiona = new float[] { 0.0f, 0.0f, 0.0f };
        private float[] pozicijaKontejnera = new float[] { 0.0f, 0.0f, 0.0f };
        private float rotacijaKamiona = 0.0f;
        private float[] rotacijaKontejnera = new float[] { 0.0f, 0.0f, 0.0f };
        private float[] rotacijaKontejnera2 = new float[] { 0.0f, 0.0f, 0.0f };
        private float[] rotacijaKontejnera3 = new float[] { 0.0f, 0.0f, 0.0f };
        
        private float brzinaAnimacije = 5.0f;
        public OpenGL gl;

        public bool disableAnimation = false;

        //teksture
        private enum TextureObjects { Metal = 0, Asphalt, Empty, Container };
        private readonly int m_textureCount = Enum.GetNames(typeof(TextureObjects)).Length;
        private uint[] m_textures = null;

        private string[] m_textureFiles = { "..//..//Textures//metal.jpg", "..//..//Textures//asphalt.jpg", "..//..//Textures//empty.png", "..//..//Textures//container.jpg" };
        public float brojPonavljanjaTeksturePuta = 30.0f;

        public float faktorSkaliranjaPodloge = 20.0f;
        public float faktorSkaliranjaBandere = 100.0f;


        #endregion Atributi

        #region Properties

        /// <summary>
        ///	 Scena koja se prikazuje.
        /// </summary>
        public AssimpScene Scene
        {
            get { return m_scene; }
            set { m_scene = value; }
        }
        public AssimpScene Scene2
        {
            get { return m_scene2; }
            set { m_scene2 = value; }
        }

        public void changeHeight(double val)
        {
            lampLightPos[1] = (float)val;
        }

        public double CylinderFactor
        {
            get { return cylinderFactor; }
            set { cylinderFactor = value; }
        }

        public double CubeFactor
        {
            get { return cubeFactor; }
            set { cubeFactor = value; }
        }

        public bool Animation
        {
            get { return animation; }
            set { animation = value; }
        }

        public float RotacijaKamiona
        {
            get { return rotacijaKamiona; }
            set { rotacijaKamiona = value; }
        }

        public float[] RotacijaKontejnera
        {
            get { return rotacijaKontejnera; }
            set { rotacijaKontejnera = value; }
        }

        public float[] RotacijaKontejnera2
        {
            get { return rotacijaKontejnera2; }
            set { rotacijaKontejnera2 = value; }
        }

        public float BrzinaAnimacije
        {
            get { return brzinaAnimacije; }
            set { brzinaAnimacije = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko X ose.
        /// </summary>
        public float RotationX
        {
            get { return m_xRotation; }
            set { m_xRotation = value; }
        }

        /// <summary>
        ///	 Ugao rotacije sveta oko Y ose.
        /// </summary>
        public float RotationY
        {
            get { return m_yRotation; }
            set { m_yRotation = value; }
        }

        /// <summary>
        ///	 Udaljenost scene od kamere.
        /// </summary>
        public float SceneDistance
        {
            get { return m_sceneDistance; }
            set { m_sceneDistance = value; }
        }

        /// <summary>
        ///	 Sirina OpenGL kontrole u pikselima.
        /// </summary>
        public int Width
        {
            get { return m_width; }
            set { m_width = value; }
        }

        /// <summary>
        ///	 Visina OpenGL kontrole u pikselima.
        /// </summary>
        public int Height
        {
            get { return m_height; }
            set { m_height = value; }
        }

        #endregion Properties

        #region Konstruktori

        /// <summary>
        ///  Konstruktor klase World.
        /// </summary>
        public World(String scenePath, String sceneFileName, int width, int height, OpenGL gl)
        {
            this.gl = gl;
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_width = width;
            this.m_height = height;
        }

        public World(String scenePath, String sceneFileName, String scenePath2, String sceneFileName2, int width, int height, OpenGL gl)
        {
            this.gl = gl;
            m_textures = new uint[m_textureCount];
            this.m_scene = new AssimpScene(scenePath, sceneFileName, gl);
            this.m_scene2 = new AssimpScene(scenePath2, sceneFileName2, gl);
            this.m_width = width;
            this.m_height = height;
        }

        /// <summary>
        ///  Destruktor klase World.
        /// </summary>
        ~World()
        {
            this.Dispose(false);
        }

        #endregion Konstruktori

        #region Metode

        /// <summary>
        ///  Korisnicka inicijalizacija i podesavanje OpenGL parametara.
        /// </summary>
        public void Initialize(OpenGL gl)
        {
            // Model sencenja na flat (konstantno)
            //gl.ShadeModel(OpenGL.GL_FLAT);
            gl.ShadeModel(OpenGL.GL_SMOOTH);
            //ukljuceno testiranje dubine
            gl.Enable(OpenGL.GL_DEPTH_TEST);
            //sakrivanje nevidljiv povrsina
            gl.Enable(OpenGL.GL_CULL_FACE);
            //color tracking
            gl.Enable(OpenGL.GL_COLOR_MATERIAL);
            //ambientalna i difuzna komponenta
            gl.ColorMaterial(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT_AND_DIFFUSE);
            //automatska normalizacija
            gl.Enable(OpenGL.GL_NORMALIZE);
            
            gl.Enable(OpenGL.GL_TEXTURE_2D);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_ADD);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_S, OpenGL.GL_REPEAT);
            gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_WRAP_T, OpenGL.GL_REPEAT);

            gl.TexGen(OpenGL.GL_S, OpenGL.GL_TEXTURE_GEN_MODE, OpenGL.GL_SPHERE_MAP);
            gl.TexGen(OpenGL.GL_T, OpenGL.GL_TEXTURE_GEN_MODE, OpenGL.GL_SPHERE_MAP);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_S);
            gl.Enable(OpenGL.GL_TEXTURE_GEN_T);

            // Ucitaj slike i kreiraj teksture
            gl.GenTextures(m_textureCount, m_textures);
            for (int i = 0; i < m_textureCount; ++i)
            {
                // Pridruzi teksturu odgovarajucem identifikatoru
                gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[i]);

                // Ucitaj sliku i podesi parametre teksture
                Bitmap image = new Bitmap(m_textureFiles[i]);
                // rotiramo sliku zbog koordinantog sistema opengl-a
                image.RotateFlip(RotateFlipType.RotateNoneFlipY);
                Rectangle rect = new Rectangle(0, 0, image.Width, image.Height);
                // RGBA format (dozvoljena providnost slike tj. alfa kanal)
                BitmapData imageData = image.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadOnly,
                                                      System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                gl.Build2DMipmaps(OpenGL.GL_TEXTURE_2D, (int)OpenGL.GL_RGBA8, image.Width, image.Height, OpenGL.GL_BGRA, OpenGL.GL_UNSIGNED_BYTE, imageData.Scan0);
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MIN_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);		// Linear Filtering
                gl.TexParameter(OpenGL.GL_TEXTURE_2D, OpenGL.GL_TEXTURE_MAG_FILTER, OpenGL.GL_LINEAR_MIPMAP_LINEAR);		// Linear Filtering


                image.UnlockBits(imageData);
                image.Dispose();
            }

            //ucitavanje modela
            m_scene.LoadScene();
            m_scene.Initialize();
            m_scene2.LoadScene();
            m_scene2.Initialize();
        }

        /// <summary>
        ///  Iscrtavanje OpenGL kontrole.
        /// </summary>
        public void Draw(OpenGL gl)
        {
            gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
            
            //podsecavanje svetla na banderi
            lampLight(gl);
            //podesavanje reflektorskog svetla iznad kontejnera
            containerLight(gl);
            gl.Enable(OpenGL.GL_LIGHTING);

            //glavna projekcija - perspektiva
            setMainProjection(gl);
            //crtanje svih objekata u prostoru
            //gl.MatrixMode(OpenGL.GL_MODELVIEW);
            //gl.Rotate(m_yRotation, m_xRotation, 0.0f);
            
            gl.Rotate(m_xRotation, 0.0f, 1.0f, 0.0f);
            gl.Rotate(m_yRotation, 1.0f, 0.0f, 0.0f);
            //gl.Translate(0.0f, 0.0f, -m_sceneDistance);
            gl.Translate(0.0f, -m_yRotation, -m_sceneDistance);
            


            drawScene(gl);
            //projekcija za 3dtext - ortho2d
            set3DTextProjection(gl);
            //crtanje  3d teksta
            draw3DText(gl);

         

            gl.Flush();
        }
        //ostale metode
        public void drawScene(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            //gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);
            //gl.Color(1.0f, 1.0f, 1.0f, 1.0f);
            //gl.Color(0.0f, 0.0f, 0.0f, 1.0f);
            
            //pocetak iscrtavanja
            gl.PushMatrix();

            //kamion
            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Empty]);
            gl.Translate(10.0f + pozicijaKamiona[0], -5.0f + pozicijaKamiona[1], -80.0f + pozicijaKamiona[2]);
            gl.Rotate(0.0f, rotacijaKamiona, 0.0f);
            m_scene.Draw();
            gl.PopMatrix();
            
            
            //kontejner
            gl.PushMatrix();
            //gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Empty]);
            //gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL);
            gl.Translate(pozicijaKontejnera[0] - 10.0f, pozicijaKontejnera[1] - 5.0f, -80.0f + pozicijaKontejnera[2]);
            gl.Scale(0.05f, 0.05f, 0.05f);
            gl.Rotate(rotacijaKontejnera[0], rotacijaKontejnera[1], rotacijaKontejnera[2]);
            gl.Rotate(rotacijaKontejnera2[0], rotacijaKontejnera2[1], rotacijaKontejnera2[2]);
            gl.Rotate(rotacijaKontejnera3[0], rotacijaKontejnera3[1], rotacijaKontejnera3[2]);
            m_scene2.Draw();
            gl.PopMatrix();


            //gl.ClearColor(1.0f, 1.0f, 1.0f, 1.0f);


            //podloga
            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Asphalt]);
            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(faktorSkaliranjaPodloge, faktorSkaliranjaPodloge, faktorSkaliranjaPodloge);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            
            gl.Translate(-80.0f, -5.0f, 0.0f);
            gl.Begin(OpenGL.GL_QUADS);
            gl.Normal(0, 1, 0);
            gl.TexCoord(0.0f, 0.0f);
            gl.Vertex(0.0f, 0.0f, 0.0f);
            gl.TexCoord(0.0f, brojPonavljanjaTeksturePuta);
            gl.Vertex(200.0f, 0.0f, 0.0f);
            gl.TexCoord(brojPonavljanjaTeksturePuta, brojPonavljanjaTeksturePuta);
            gl.Vertex(200.0f, 0.0f, -200.0f);
            gl.TexCoord(brojPonavljanjaTeksturePuta, 0.0f);
            gl.Vertex(0.0f, 0.0f, -200.0f);
            gl.End();
            gl.PopMatrix();
            
            
            //zidovi sa strana kontejnera
            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Empty]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);
            gl.Translate(0.0f, 0.0f, -80.0f);
            gl.Scale(1f, 8f, 10f);
            cube = new Cube();
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(-25.0f, 0.0f, -80.0f);
            gl.Scale(1f, 8f, 10f);
            cube = new Cube();
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();
            
            
            //bandera
            gl.PushMatrix();
            gl.BindTexture(OpenGL.GL_TEXTURE_2D, m_textures[(int)TextureObjects.Metal]);
            gl.TexEnv(OpenGL.GL_TEXTURE_ENV, OpenGL.GL_TEXTURE_ENV_MODE, OpenGL.GL_MODULATE);

            gl.MatrixMode(OpenGL.GL_TEXTURE);
            gl.LoadIdentity();
            gl.Scale(faktorSkaliranjaBandere, faktorSkaliranjaBandere, faktorSkaliranjaBandere);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);

           
            gl.Translate(20.0f, -5.0f, -80.0f);
            gl.Rotate(-90.0f, 0.0f, 0.0f);
            gl.Scale(cylinderFactor, cylinderFactor, cylinderFactor);
            cylinder = new Cylinder();
            cylinder.Height = lampLightPos[1];
            cylinder.BaseRadius = 0.5f;
            cylinder.TopRadius = 0.5f;
            cylinder.CreateInContext(gl);    
            cylinder.Render(gl, RenderMode.Render);
            cylinder.CreateInContext(gl);    
            
            gl.PopMatrix();

            gl.PushMatrix();
            gl.Translate(lampLightPos[0], lampLightPos[1] - 5.0f, lampLightPos[2]);
            gl.Scale(cubeFactor, cubeFactor, cubeFactor);
            cube = new Cube();
            cube.Render(gl, RenderMode.Render);
            gl.PopMatrix();


            //kraj iscrtavanja
            gl.PopMatrix();
        }
        public void draw3DText(OpenGL gl)
        {
            float textx = 0.0f;
            float textz = 0.0f;
            float texty = -20.0f;
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.PushMatrix();
            gl.Scale(5.0f, 5.0f, 5.0f);
            gl.Color(1f, 1f, 0f);
            //gl.LineWidth(0.5f);
            gl.PushMatrix();
            gl.Translate(textx, texty, textz);
            gl.DrawText3D("Helvetica Bold", 14f, 0f, 0f, "Predmet: Racunarska grafika");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(textx, texty - 1.0f, textz);
            gl.DrawText3D("Helvetica Bold", 14f, 0f, 0f, "Sk.god: 2017/18.");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(textx, texty - 2.0f, textz);
            gl.DrawText3D("Helvetica Bold", 14f, 0f, 0f, "Ime: Milos");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(textx, texty - 3.0f, textz);
            gl.DrawText3D("Helvetica Bold", 14f, 0f, 0f, "Prezime: Tepic");
            gl.PopMatrix();
            gl.PushMatrix();
            gl.Translate(textx, texty - 4.0f, textz);
            gl.DrawText3D("Helvetica Bold", 14f, 0f, 0f, "Sifra zad: 1.1");
            gl.PopMatrix();
            gl.PopMatrix();
        }
        public void setMainProjection(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Perspective(45f, (double)m_width / m_height, 1f, 20000f);
            gl.LookAt(5.0f, 20.0f, -30.0f, 0.0f, 0.0f, -80.0f, 0.0f, 1000.0f, 0.0f);
            gl.Viewport(0, 0, m_width, m_height);
        }
        public void set3DTextProjection(OpenGL gl)
        {
            gl.MatrixMode(OpenGL.GL_PROJECTION);
            gl.LoadIdentity();
            gl.Ortho2D(-100.0f, 60.0f, -130.0f, 50.0f);
            //gl.Viewport((m_width / 4) * 3, (m_height / 4) * 3, m_width / 4, m_height / 4);
        }
        public void lampLight(OpenGL gl)
        {
            float[] ambientMaterial = new float [] { 0.11f, 0.06f, 0.11f, 1.0f };
            float[] difuseMateral = new float[] { 0.43f, 0.47f, 0.54f, 1.0f };
            float[] specularMaterial = new float[] { 0.67f, 0.56f, 0.52f, 1.0f };
            float[] emissionMaterial = new float[] { 0.0f, 0.0f, 0.0f, 0.0f };
            float shiness = 10.0f;

            float[] ambijentalnaKomponenta = { 0.3f, 0.3f, 0.3f, 1.0f };
            float[] difuznaKomponenta = { 0.7f, 0.7f, 0.7f, 1.0f };
            float[] spekularnaKomponenta = new float[] { 1.0f, 1.0f, 1.0f, 1.0f };
            float[] pozicijaSvetla = new float[] { lampLightPos[0] + 40.0f, lampLightPos[1] - 5.0f, lampLightPos[2] + 80.0f , 1.0f};

            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_AMBIENT, ambientMaterial);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_DIFFUSE, difuseMateral);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SPECULAR, specularMaterial);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_EMISSION, emissionMaterial);
            gl.Material(OpenGL.GL_FRONT, OpenGL.GL_SHININESS, shiness);

            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_POSITION, pozicijaSvetla);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPECULAR, spekularnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_SPOT_CUTOFF, 180.0f);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_AMBIENT, ambijentalnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT0, OpenGL.GL_DIFFUSE, difuznaKomponenta);

            gl.Enable(OpenGL.GL_LIGHT0);
        }
        public void containerLight(OpenGL gl)
        {
            float[] ambijentalnaKomponenta = { 0.3f, 0.3f, 0.0f, 1.0f };
            float[] difuznaKomponenta = { 0.7f, 0.7f, 0.0f, 1.0f };
            float[] spekularnaKomponenta = { 1.0f, 1.0f, 0.0f, 1.0f };
            float[] smer = { 0.0f, -1.0f, 0.0f };
            // Pridruži komponente svetlosnom izvoru 0
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_AMBIENT, ambijentalnaKomponenta);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_DIFFUSE, difuznaKomponenta);
            //gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPECULAR, spekularnaKomponenta);
            // Podesi parametre reflektorkskog izvora
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_DIRECTION, smer);
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_SPOT_CUTOFF, 35.0f);
            // Ukljuci svetlosni izvor
            gl.Enable(OpenGL.GL_LIGHT1);
            // Pozicioniraj svetlosni izvor
            float[] pozicija = { -15.0f, 25.0f, -80.0f, 1.0f };
            gl.Light(OpenGL.GL_LIGHT1, OpenGL.GL_POSITION, pozicija);
        }

        public void startAnimation()
        {
            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromMilliseconds(2);
            timer1.Tick += new EventHandler(animateTruck);
            timer1.Tick += new EventHandler(animateContainer);
            timer1.Start();

            timer2 = new DispatcherTimer();
            timer2.Interval = TimeSpan.FromMilliseconds(2);
            timer2.Tick += new EventHandler(animateContainerRotation);
            timer2.Start();
        }
        private void animateTruck(object sender, EventArgs e)
        {
            //napred
            if(pozicijaKamiona[2] <= 30.0f)
                pozicijaKamiona[2] += 1.0f;
            //rotacija na levo
            if (pozicijaKamiona[2] >= 30.0f && rotacijaKamiona >= -90.0f)
            {
                rotacijaKamiona -= 1.0f;
            }
            if (pozicijaKamiona[2] >= 30.0f && rotacijaKamiona <= -90.0f && pozicijaKamiona[0] >= -50.0f)
            {
                pozicijaKamiona[0] -= 0.5f;
            }
        }
        private void animateContainer(object sender, EventArgs e)
        {
            if (pozicijaKamiona[0] <= -50.0f && pozicijaKontejnera[2] <= 30.0f)
            {
                pozicijaKontejnera[2] += 0.5f;
            }

            if (pozicijaKontejnera[2] >= 30.0f && rotacijaKontejnera[1] >= -90.0f)
            {
                rotacijaKontejnera[1] -= 1.0f;
            }
            if (rotacijaKontejnera[1] <= -90.0f && pozicijaKontejnera[0] >= -13.0f)
            {
                pozicijaKontejnera[0] -= 0.5f;
            }
            if (pozicijaKontejnera[0] <= -13.0f && pozicijaKontejnera[1] <= 10.0f)
            {
                pozicijaKontejnera[1] += 0.5f;
            }
        }
        private void animateContainerRotation(object sender, EventArgs e)
        {
            if (pozicijaKontejnera[0] <= -13.0f && pozicijaKontejnera[1] >= 10.0f && rotacijaKontejnera2[0] <= 90.0f)
            {
                
                rotacijaKontejnera2[0] += brzinaAnimacije;
            }
            if (rotacijaKontejnera2[0] >= 90.0f && rotacijaKontejnera3[0] >= -90.0f)
            {
                
                rotacijaKontejnera3[0] -= brzinaAnimacije;
            }

            if (rotacijaKontejnera3[0] <= -90.0f && pozicijaKontejnera[1] >= 0.0f)
            {
                pozicijaKontejnera[1] -= 1.0f;
            }

            if (rotacijaKontejnera3[0] <= -90.0f && pozicijaKontejnera[1] <= 0.0f)
            {
                pozicijaKamiona[0] -= 0.5f;
            }

            if (pozicijaKamiona[0] <= -60.0f)
            {
                timer1.Stop();
                timer2.Stop();
                disableAnimation = false;
                pozicijaKamiona = new float[] { 0.0f, 0.0f, 0.0f };
                pozicijaKontejnera = new float[] { 0.0f, 0.0f, 0.0f };
                rotacijaKamiona = 0.0f;
                rotacijaKontejnera = new float[] { 0.0f, 0.0f, 0.0f };
                rotacijaKontejnera2 = new float[] { 0.0f, 0.0f, 0.0f };
                rotacijaKontejnera3 = new float[] { 0.0f, 0.0f, 0.0f };   
            }
        }
     
        /// <summary>
        /// Podesava viewport i projekciju za OpenGL kontrolu.
        /// </summary>
        public void Resize(OpenGL gl, int width, int height)
        {
            m_width = width;
            m_height = height;
            gl.MatrixMode(OpenGL.GL_PROJECTION);      // selektuj Projection Matrix
            gl.LoadIdentity();
            gl.Perspective(45f, m_width / m_height, 0.1f, 20000f);
            gl.Viewport(0, 0, m_width, m_height);
            gl.MatrixMode(OpenGL.GL_MODELVIEW);
            gl.LoadIdentity();                // resetuj ModelView Matrix
        }

        /// <summary>
        ///  Implementacija IDisposable interfejsa.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_scene.Dispose();
            }
        }

        #endregion Metode

        #region IDisposable metode

        /// <summary>
        ///  Dispose metoda.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable metode
    }
}
