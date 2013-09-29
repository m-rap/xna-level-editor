﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using EditorModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace View
{
    public partial class Editor : UserControl, IObserver
    {
        #region attributes
        private MapModel mapModel;
        private MainUserControl mainUserControl;
        private Camera camera;
        private ContentBuilder contentBuilder;
        private DrawingObject selected;
        private ModelBoundingBox selectedBoundingBox;
        private EditorMode editorMode;
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;
        private Terrain terrain;
        private string text;
        private TerrainPointer terrainPointer;
        private TerrainBrush terrainBrush;
        private BasicEffect basicEffect;
        private Effect terrainEffect;
        private bool moveForward;
        private bool moveBackward;
        private bool moveLeft;
        private bool moveRight;
        private ContentManager contentManager;
        private Texture2D heightMap;
        private Texture2D brushHeightMap;
        private bool mouseMoving;
        private MouseEventArgs mouseEventArgs;
        private int tempMouseX;
        private int tempMouseY;
        private ContentManager heightmapContent;
        private string effectFile;
        #endregion

        public MapModel MapModel
        {
            get { return mapModel; }
            set { mapModel = value; }
        }

        public MainUserControl MainUserControl
        {
            get { return mainUserControl; }
            set
            {
                mainUserControl = value;
            }
        }

        public Camera Camera
        {
            get { return camera; }
        }

        public ContentBuilder ContentBuilder
        {
            get { return contentBuilder; }
        }

        public DrawingObject Selected
        {
            get { return selected; }
            set { selected = value; }
        }

        public ModelBoundingBox SelectedBoundingBox
        {
            get { return selectedBoundingBox; }
        }

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return graphicsDeviceControl1.GraphicsDevice;
            }
        }

        public SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        public SpriteFont SpriteFont
        {
            get { return spriteFont; }
        }

        public Terrain Terrain
        {
            get { return terrain; }
        }

        public string Text1
        {
            get { return text; }
            set { text = value; }
        }

        public TerrainPointer TerrainPointer
        {
            get { return terrainPointer; }
            set { terrainPointer = value; }
        }

        public TerrainBrush TerrainBrush
        {
            get { return terrainBrush; }
            set { terrainBrush = value; }
        }

        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = System.Reflection.Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return System.IO.Path.GetDirectoryName(path);
            }
        }

        public Editor()
        {
            InitializeComponent();
        }

        private void Editor_Load(object sender, EventArgs e)
        {
            mouseMoving = false;
            mouseEventArgs = null;
            tempMouseX = tempMouseY = -1;
            text = "";
            basicEffect = new BasicEffect(graphicsDeviceControl1.GraphicsDevice);
            contentBuilder = ContentBuilder.Instance;
            contentManager = new ContentManager(graphicsDeviceControl1.Services, contentBuilder.OutputDirectory);
            heightmapContent = new ContentManager(graphicsDeviceControl1.Services, contentBuilder.OutputDirectory);
            spriteBatch = new SpriteBatch(graphicsDeviceControl1.GraphicsDevice);
            
            string errorBuild = "";
            effectFile = AssemblyDirectory + "\\effects.fx";
            try
            {
                //importer reference: http://msdn.microsoft.com/en-us/library/bb447762%28v=xnagamestudio.20%29.aspx
                contentBuilder.Add(AssemblyDirectory + "\\SegoeUI.spritefont", "SegoeUI.spritefont", null, "FontDescriptionProcessor");
                contentBuilder.Add(effectFile, "effects", null, "EffectProcessor");
                contentBuilder.Add(AssemblyDirectory + "\\heightmap.png", "heightmap", null, "TextureProcessor");
                contentBuilder.Add(AssemblyDirectory + "\\brush.bmp", "brush", null, "TextureProcessor");
                errorBuild = contentBuilder.Build();
                spriteFont = contentManager.Load<SpriteFont>("SegoeUI.spritefont");
                terrainEffect = contentManager.Load<Effect>("effects");
                heightMap = heightmapContent.Load<Texture2D>("heightmap");
                brushHeightMap = contentManager.Load<Texture2D>("brush");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace + "\r\n" + errorBuild);
            }

            try
            {
                camera = new Camera(GraphicsDevice);
                camera.Position = new Vector3(0, 50, 0);
                camera.AspectRatio = graphicsDeviceControl1.GraphicsDevice.Viewport.AspectRatio;
                camera.Rotate(20, 45, 0);
                camera.Attach(this);
                CheckIsOrthographic();

                heightMap = null;
                string heightMapFile = AssemblyDirectory + "\\test_HeightMap.png";
                if (heightMap != null)
                    terrain = new Terrain(GraphicsDevice, camera, heightMap, heightMapFile, effectFile);
                else
                    terrain = new Terrain(GraphicsDevice, camera, heightMapFile, effectFile);

                //impossible
                //if (mainUserControl == null)
                    //terrain.HeightMapFile = AssemblyDirectory + "\\" + mainUserControl._ClassManager.Name + ".png";
                //else
                    //terrain.HeightMapFile = AssemblyDirectory + "\\test_HeightMap.png";

                editorMode = new EditorMode_Select(this);

                selected = null;
                selectedBoundingBox = new ModelBoundingBox(graphicsDeviceControl1.GraphicsDevice, camera);
                selectedBoundingBox.SpriteBatch = spriteBatch;
                selectedBoundingBox.SpriteFont = spriteFont;
                CheckActiveTransformMode();

                terrainPointer = new TerrainPointer(terrain);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace + "\r\n" + errorBuild);
            }
            try
            {
                terrainBrush = new TerrainBrush(GraphicsDevice, terrain, brushHeightMap);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace + "\r\n" + errorBuild);
            }

            camera.Notify();
        }

        void IObserver.UpdateObserver()
        {
            graphicsDeviceControl1.Invalidate();
        }

        public void ImportHeightmap(string heightmapFile)
        {
            heightmapContent = new ContentManager(graphicsDeviceControl1.Services, contentBuilder.OutputDirectory);
            contentBuilder.Add(heightmapFile, "heightmap", null, "TextureProcessor");
            contentBuilder.Build();
            heightMap = heightmapContent.Load<Texture2D>("heightmap");
            camera.Detach(terrain.TerrainIndexer);
            terrain = new Terrain(GraphicsDevice, camera, heightMap, heightmapFile, effectFile);
            terrainBrush = new TerrainBrush(GraphicsDevice, terrain, brushHeightMap);
            camera.Notify();
        }

        public void AddObject(string file, string name, Vector3 position, Vector3 eulerRotation)
        {
            DrawingObject obj = new DrawingObject();

            string originalName = name;

            for (int i = 1; ; ++i)
            {
                if (!mapModel.NameExists(name))
                    break;
                name = originalName + "_" + i;
            }
            try
            {
                obj.DrawingModel = OpenModel(file);
                obj.Name = name;
                obj.Position = position;
                obj.EulerRotation = eulerRotation;
                obj.SourceFile = file;
                obj.Attach(this);
                mapModel.Objects.Add(obj);
                XleGenerator.CodeLines codeLines = new XleGenerator.CodeLines();
                codeLines.Model = obj;
                mainUserControl._ClassManager.CodeLinesList.Add(codeLines);
                obj.Notify();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        public Model LoadModel()
        {
            if (openFileDialog1.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            return OpenModel(openFileDialog1.FileName);
        }

        public Model OpenModel(string path)
        {
            bool isAdded = false;
            foreach (Microsoft.Build.Evaluation.ProjectItem item in contentBuilder.ProjectItems)
            {
                foreach (Microsoft.Build.Evaluation.ProjectMetadata metadata in item.Metadata)
                {
                    if (metadata.Name == "Link" && metadata.EvaluatedValue == System.IO.Path.GetFileName(path))
                    {
                        isAdded = true;
                    }
                }
            }

            string name = System.IO.Path.GetFileNameWithoutExtension(path);

            if (!isAdded)
            {
                contentBuilder.Add(path, name, null, "ModelProcessor");
                string errorBuild = contentBuilder.Build();
                if (string.IsNullOrEmpty(errorBuild))
                {
                    Model model = contentManager.Load<Model>(name);
                    graphicsDeviceControl1.Invalidate();
                    return model;
                }
                else
                {
                    contentBuilder.Remove(name);
                    throw new ArgumentException(errorBuild);
                }
            }
            else
            {
                Model model = contentManager.Load<Model>(name);
                graphicsDeviceControl1.Invalidate();
                return model;
            }
            return null;
        }
        
        private void graphicsDeviceControl1_Paint(object sender, PaintEventArgs e)
        {
            graphicsDeviceControl1.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Microsoft.Xna.Framework.Color.CornflowerBlue, 1.0f, 0);

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            if (mapModel != null)
                foreach (DrawingObject obj in mapModel.Objects)
                    obj.Draw(camera.World, camera.Projection, false, camera.Direction);
            
            basicEffect.View = camera.World;
            basicEffect.Projection = camera.Projection;

            try
            {
                if (terrainEffect == null)
                    terrain.Draw(basicEffect);
                else
                    terrain.Draw(terrainEffect);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
                terrain.Draw(basicEffect);
            }

            if (editorMode is EditorMode_Terrain)
            {
                //terrainPointer.Draw(basicEffect, GraphicsDevice);
                //text += terrainPointer.Text;
                terrainBrush.Draw(basicEffect);
                text += terrainBrush.Text;
            }

            if (selected != null)
            {
                selectedBoundingBox.Draw(ref basicEffect);
                text += selectedBoundingBox.AxisLines.Text + "\r\n";
            }

            text += camera.Zoom + "\r\n";

            if (spriteFont != null)
            {
                spriteBatch.Begin();
                text += mouseMoving + "\r\n";
                if (mouseEventArgs != null)
                {
                    text += mouseEventArgs.X + "," + mouseEventArgs.Y + "\r\n";
                }
                spriteBatch.DrawString(spriteFont, text, new Vector2(50, 50), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1.0f, SpriteEffects.None, 0.0f);
                spriteBatch.End();
            }

            text = "";
        }

        private void graphicsDeviceControl1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                moveForward = true;
            if (e.KeyCode == Keys.S)
                moveBackward = true;
            if (e.KeyCode == Keys.A)
                moveLeft = true;
            if (e.KeyCode == Keys.D)
                moveRight = true;
            if (moveForward || moveBackward || moveLeft || moveRight)
                timer1.Enabled = true;
        }

        private void graphicsDeviceControl1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.W)
                moveForward = false;
            if (e.KeyCode == Keys.S)
                moveBackward = false;
            if (e.KeyCode == Keys.A)
                moveLeft = false;
            if (e.KeyCode == Keys.D)
                moveRight = false;
            if (!(moveForward || moveBackward || moveLeft || moveRight))
            {
                timer1.Enabled = false;
                camera.IsMoving = false;
                camera.Notify();
            }
        }

        private void graphicsDeviceControl1_MouseDown(object sender, MouseEventArgs e)
        {
            editorMode.MouseDown(sender, e);
        }

        private void graphicsDeviceControl1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (!mouseMoving)
                    mouseMoving = true;

                if (!timer1.Enabled)
                    timer1.Enabled = true;

                mouseEventArgs = e;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
        }

        private void graphicsDeviceControl1_MouseUp(object sender, MouseEventArgs e)
        {
            editorMode.MouseUp(sender, e);
        }
        
        private void graphicsDeviceControl1_DragEnter(object sender, DragEventArgs e)
        {
            editorMode.DragEnter(sender, e);
        }

        private void graphicsDeviceControl1_DragDrop(object sender, DragEventArgs e)
        {
            editorMode.DragDrop(sender, e);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            float speed = (float)1;
            if (moveRight)
                camera.MoveRight(speed);
            if (moveLeft)
                camera.MoveRight(-speed);
            if (moveForward)
            {
                camera.MoveForward(speed);
                camera.Zoom += 5;
            }
            if (moveBackward)
            {
                camera.MoveForward(-speed);
                camera.Zoom -= 5;
            }
            if (mouseMoving)
            {
                if (mouseEventArgs != null && mouseEventArgs.X == tempMouseX && mouseEventArgs.Y == tempMouseY)
                {
                    mouseMoving = false;
                    mouseEventArgs = null;
                    tempMouseX = tempMouseY = -1;
                    timer1.Enabled = false;
                    camera.Notify();
                }
                else
                {
                    editorMode.MouseMove(sender, mouseEventArgs);

                    if (mouseEventArgs != null)
                    {
                        tempMouseX = mouseEventArgs.X;
                        tempMouseY = mouseEventArgs.Y;
                    }

                    //Thread thread = new Thread(() => editorMode.MouseMove(sender, mouseEventArgs));
                    //thread.Start();
                }
            }
            if (moveRight || moveLeft || moveForward || moveBackward)
                camera.Notify();
        }

        private void graphicsDeviceControl1_Resize(object sender, EventArgs e)
        {
            if (camera == null || graphicsDeviceControl1.GraphicsDevice == null)
                return;
            camera.AspectRatio = graphicsDeviceControl1.GraphicsDevice.Viewport.AspectRatio;
            terrain.TerrainIndexer.Resize();
            camera.Notify();
        }

        private void translateModeToolStripButton_Click(object sender, EventArgs e)
        {
            if (selected != null)
                selectedBoundingBox.ChangeAxisLines(1);
            CheckActiveTransformMode();
        }

        private void rotateModeToolStripButton_Click(object sender, EventArgs e)
        {
            if (selected != null)
                selectedBoundingBox.ChangeAxisLines(2);
            CheckActiveTransformMode();
        }

        private void CheckActiveTransformMode()
        {
            if (selected == null)
            {
                translateModeToolStripButton.Checked = false;
                translateModeToolStripButton.CheckState = CheckState.Unchecked;
                rotateModeToolStripButton.Checked = false;
                rotateModeToolStripButton.CheckState = CheckState.Unchecked;
            }
            if (selectedBoundingBox.AxisLines.GetType() == typeof(RotationAxisLines))
            {
                translateModeToolStripButton.Checked = false;
                translateModeToolStripButton.CheckState = CheckState.Unchecked;
                rotateModeToolStripButton.Checked = true;
                rotateModeToolStripButton.CheckState = CheckState.Checked;
            }
            else
            {
                translateModeToolStripButton.Checked = true;
                translateModeToolStripButton.CheckState = CheckState.Checked;
                rotateModeToolStripButton.Checked = false;
                rotateModeToolStripButton.CheckState = CheckState.Unchecked;
            }
            camera.Notify();
        }

        private void selectModeStripButton_Click(object sender, EventArgs e)
        {
            editorMode = new EditorMode_Select(this);
            CheckEditorMode();
        }

        private void terrainIncreaseToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                editorMode = new EditorMode_Terrain_IncreaseHeight(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            CheckEditorMode();
        }

        private void terrainDecreaseToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                editorMode = new EditorMode_Terrain_DecreaseHeight(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n" + ex.StackTrace);
            }
            CheckEditorMode();
        }

        private void CheckEditorMode()
        {
            if (selected != null)
            {
                selected.Detach(mainUserControl.ObjectProperties1);
                mainUserControl.ObjectProperties1.Model = null;
                selected = null;
            }
            if (editorMode.GetType() == typeof(EditorMode_Select))
            {
                selectModeToolStripButton.Checked = true;
                selectModeToolStripButton.CheckState = CheckState.Checked;
                terrainIncreaseToolStripButton.Checked = false;
                terrainIncreaseToolStripButton.CheckState = CheckState.Unchecked;
                terrainDecreaseToolStripButton.Checked = false;
                terrainDecreaseToolStripButton.CheckState = CheckState.Unchecked;
            }
            if (editorMode.GetType() == typeof(EditorMode_Terrain_IncreaseHeight))
            {
                selectModeToolStripButton.Checked = false;
                selectModeToolStripButton.CheckState = CheckState.Unchecked;
                terrainIncreaseToolStripButton.Checked = true;
                terrainIncreaseToolStripButton.CheckState = CheckState.Checked;
                terrainDecreaseToolStripButton.Checked = false;
                terrainDecreaseToolStripButton.CheckState = CheckState.Unchecked;
            }
            if (editorMode.GetType() == typeof(EditorMode_Terrain_DecreaseHeight))
            {
                selectModeToolStripButton.Checked = false;
                selectModeToolStripButton.CheckState = CheckState.Unchecked;
                terrainIncreaseToolStripButton.Checked = false;
                terrainIncreaseToolStripButton.CheckState = CheckState.Unchecked;
                terrainDecreaseToolStripButton.Checked = true;
                terrainDecreaseToolStripButton.CheckState = CheckState.Checked;
            }
            camera.Notify();
        }

        private void orthogonalStripButton_Click(object sender, EventArgs e)
        {
            camera.IsOrthographic = !camera.IsOrthographic;
            CheckIsOrthographic();
            camera.Notify();
        }

        private void CheckIsOrthographic()
        {
            if (camera.IsOrthographic)
            {
                orthogonalStripButton.Checked = true;
                orthogonalStripButton.CheckState = CheckState.Checked;
            }
            else
            {
                orthogonalStripButton.Checked = false;
                orthogonalStripButton.CheckState = CheckState.Unchecked;
            }
        }

        public void SelectObject(DrawingObject temp)
        {
            selected = temp;
            selectedBoundingBox.Model = selected;
            selected.Attach(mainUserControl.ObjectProperties1);
            mainUserControl.ObjectProperties1.Model = selected;
            selected.Notify();
        }

        public void DeselectObject()
        {
            if (selected != null)
            {
                selected.Detach(mainUserControl.ObjectProperties1);
                mainUserControl.ObjectProperties1.Model = null;
                selected = null;
            }
        }
    }
}
