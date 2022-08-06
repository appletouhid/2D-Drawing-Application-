using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace GrafPack
{
    public partial class GrafPack : Form
    {
        private Point SelectedVertex;
        private Color SelectedColor;
        private Bitmap RenderTarget;
        private List<Point> DrawLine;

        private Graphics Graphics;
        private MatrixMxN VertexSelectionHighlight;
        private OpenFileDialog FileDialog;
        private const double RTOD = 180 / Math.PI;
        private const int INVALID_INDEX = -1;
        private const int FRAME_COUNT = 2;
        private const double SELECT_VERTEX_THRESHOLD = 10;

        private List<Shape> Shapes = new List<Shape>();
        private Type ObjectType = null;
        private Shape CreationObject = null;
        private string Filename = null;
        private int SelectedIndex = INVALID_INDEX;
        private int SelectedVertexId = INVALID_INDEX;

        // Menu Items
        private MenuItem fileItem;
        private MenuItem createItem;
        private MenuItem submodeItem;
        private MenuItem objectSelect;
        private MenuItem vertexSelect;
        private MenuItem moveItem;
        private MenuItem rotateItem;
        private MenuItem scaleItem;
        private MenuItem deleteItem;
        private MenuItem clearItem;
        private MenuItem extendVertexItem;
        private MenuItem drawVertexItem;
        private MenuItem colorItem;

        private Point MouseLastClicked;
        private Point MousePrevious;
        private Point MouseCurrent;

        private bool MouseMoved = false;
        private bool IsMouseDown = false;
        private bool IsSelectionClicked = false;

        enum Mode
        {
            Select,
            Create,
            Move,
            Rotate,
            Scale,
            Extend,
            Draw,
        }

        enum Submode
        {
            Object,
            Vertex
        }

        private Mode    EditMode    = Mode.Select;
        private Submode EditSubmode = Submode.Object;


        public GrafPack()
        {
            InitializeComponent();
            this.SetStyle( ControlStyles.ResizeRedraw , true );
            this.WindowState                = FormWindowState.Maximized;
            this.BackColor                  = Color.White;
            this.Graphics                   = this.CreateGraphics();
            this.DrawLine                   = new List<Point>();
            this.RenderTarget               = new Bitmap( 1 , 1 );
            this.VertexSelectionHighlight   = new MatrixMxN( 7 , 7 );
            this.FileDialog                 = new OpenFileDialog();
            this.SelectedColor              = Color.Black;
            this.Text                       = "GrafPack";

            this.VertexSelectionHighlight.Set( 0 , 2 , 1 );
            this.VertexSelectionHighlight.Set( 0 , 3 , 1 );
            this.VertexSelectionHighlight.Set( 0 , 4 , 1 );
            this.VertexSelectionHighlight.Set( 1 , 1 , 1 );
            this.VertexSelectionHighlight.Set( 1 , 2 , 1 );
            this.VertexSelectionHighlight.Set( 1 , 3 , 1 );
            this.VertexSelectionHighlight.Set( 1 , 4 , 1 );
            this.VertexSelectionHighlight.Set( 1 , 5 , 1 );
            this.VertexSelectionHighlight.Set( 2 , 0 , 1 );
            this.VertexSelectionHighlight.Set( 2 , 1 , 1 );
            this.VertexSelectionHighlight.Set( 2 , 2 , 1 );
            this.VertexSelectionHighlight.Set( 2 , 3 , 1 );
            this.VertexSelectionHighlight.Set( 2 , 4 , 1 );
            this.VertexSelectionHighlight.Set( 2 , 5 , 1 );
            this.VertexSelectionHighlight.Set( 2 , 6 , 1 );
            this.VertexSelectionHighlight.Set( 3 , 0 , 1 );
            this.VertexSelectionHighlight.Set( 3 , 1 , 1 );
            this.VertexSelectionHighlight.Set( 3 , 2 , 1 );
            this.VertexSelectionHighlight.Set( 3 , 3 , 1 );
            this.VertexSelectionHighlight.Set( 3 , 4 , 1 );
            this.VertexSelectionHighlight.Set( 3 , 5 , 1 );
            this.VertexSelectionHighlight.Set( 3 , 6 , 1 );
            this.VertexSelectionHighlight.Set( 4 , 0 , 1 );
            this.VertexSelectionHighlight.Set( 4 , 1 , 1 );
            this.VertexSelectionHighlight.Set( 4 , 2 , 1 );
            this.VertexSelectionHighlight.Set( 4 , 3 , 1 );
            this.VertexSelectionHighlight.Set( 4 , 4 , 1 );
            this.VertexSelectionHighlight.Set( 4 , 5 , 1 );
            this.VertexSelectionHighlight.Set( 4 , 6 , 1 );
            this.VertexSelectionHighlight.Set( 5 , 1 , 1 );
            this.VertexSelectionHighlight.Set( 5 , 2 , 1 );
            this.VertexSelectionHighlight.Set( 5 , 3 , 1 );
            this.VertexSelectionHighlight.Set( 5 , 4 , 1 );
            this.VertexSelectionHighlight.Set( 5 , 5 , 1 );
            this.VertexSelectionHighlight.Set( 6 , 2 , 1 );
            this.VertexSelectionHighlight.Set( 6 , 3 , 1 );
            this.VertexSelectionHighlight.Set( 6 , 4 , 1 );

            // The following approach uses menu items coupled with mouse clicks
            this.Menu            = new MainMenu();
            this.FormClosing    += OnFormClosing;
            this.KeyUp          += OnKeyUp;
            this.MouseDown      += OnMouseDown;
            this.MouseMove      += OnMouseMoved;
            this.MouseUp        += OnMouseUp;
            
            this.fileItem           = new MenuItem();
            this.createItem         = new MenuItem();
            this.submodeItem        = new MenuItem();
            this.objectSelect       = new MenuItem();
            this.vertexSelect       = new MenuItem();
            this.moveItem           = new MenuItem();
            this.rotateItem         = new MenuItem();
            this.scaleItem          = new MenuItem();
            this.deleteItem         = new MenuItem();
            this.clearItem          = new MenuItem();
            this.extendVertexItem   = new MenuItem();
            this.drawVertexItem     = new MenuItem();
            this.colorItem          = new MenuItem();
            
            MenuItem newItem        = new MenuItem();
            MenuItem openItem       = new MenuItem();
            MenuItem saveItem       = new MenuItem();
            MenuItem saveAsItem     = new MenuItem();
            MenuItem exitItem       = new MenuItem();
            MenuItem squareItem     = new MenuItem();
            MenuItem triangleItem   = new MenuItem();
            MenuItem circleItem     = new MenuItem();
          
            MenuItem blackColorItem     = new MenuItem();
            MenuItem grayColorItem      = new MenuItem();
            MenuItem brownColorItem     = new MenuItem();
            MenuItem redColorItem       = new MenuItem();
            MenuItem pinkColorItem      = new MenuItem();
            MenuItem greenColorItem     = new MenuItem();
            MenuItem orangeColorItem    = new MenuItem();
            MenuItem yellowColorItem    = new MenuItem();
            MenuItem blueColorItem      = new MenuItem();
            MenuItem purpleColorItem    = new MenuItem();


            // File Dialog
            this.FileDialog.Title = "Browse Workbase File";
            this.FileDialog.DefaultExt = ".workbase";
            this.FileDialog.Filter = "Workbase Files (*.workbase)|*.workbase|All Files (*.*)|*.*";
            this.FileDialog.FilterIndex = 0;
            this.FileDialog.InitialDirectory = Directory.GetCurrentDirectory();
            this.FileDialog.Multiselect = false;
            this.FileDialog.RestoreDirectory = true;
            this.FileDialog.CheckFileExists = true;
            this.FileDialog.CheckPathExists = true;

            newItem.Text        = "New (Ctrl+N)";
            openItem.Text       = "Open (Ctrl+O)";
            saveItem.Text       = "Save (Ctrl+S)";
            saveAsItem.Text     = "Save As";
            exitItem.Text       = "Exit";

            triangleItem.Text   = "Triangle (1)";
            squareItem.Text     = "Square (2)";
            circleItem.Text     = "Circle (3)";
            
            blackColorItem.Text     = "Black";
            grayColorItem.Text      = "Gray";
            brownColorItem.Text     = "Brown";
            redColorItem.Text       = "Red";
            pinkColorItem.Text      = "Pink";
            greenColorItem.Text     = "Green";
            orangeColorItem.Text    = "Orange";
            yellowColorItem.Text    = "Yellow";
            blueColorItem.Text      = "Blue";
            purpleColorItem.Text    = "Purple";
            
            this.fileItem.Text          = "File";
            this.createItem.Text        = "Create";
            this.submodeItem.Text       = "Select";
            this.objectSelect.Text      = "Object (Z)";
            this.vertexSelect.Text      = "Vertex (X)";
            
            this.moveItem.Text          = "Move (Q)";
            this.rotateItem.Text        = "Rotate (W)";
            this.scaleItem.Text         = "Scale (E)";
            this.deleteItem.Text        = "Delete (Del)";
            this.extendVertexItem.Text  = "Extend (S)";
            this.drawVertexItem.Text    = "Draw (C)";
            this.clearItem.Text         = "Clear (Backspace)";
            this.colorItem.Text         = "Color";
            
            this.fileItem.MenuItems.Add     ( newItem       );
            this.fileItem.MenuItems.Add     ( openItem      );
            this.fileItem.MenuItems.Add     ( saveItem      );
            this.fileItem.MenuItems.Add     ( saveAsItem    );
            this.fileItem.MenuItems.Add     ( exitItem      );

            this.createItem.MenuItems.Add   ( triangleItem  );
            this.createItem.MenuItems.Add   ( squareItem    );
            this.createItem.MenuItems.Add   ( circleItem    );
            
            this.submodeItem.MenuItems.Add  ( objectSelect  );
            this.submodeItem.MenuItems.Add  ( vertexSelect  );

            this.colorItem.MenuItems.Add    ( blackColorItem    );
            this.colorItem.MenuItems.Add    ( grayColorItem     );
            this.colorItem.MenuItems.Add    ( brownColorItem    );
            this.colorItem.MenuItems.Add    ( redColorItem      );
            this.colorItem.MenuItems.Add    ( pinkColorItem     );
            this.colorItem.MenuItems.Add    ( greenColorItem    );
            this.colorItem.MenuItems.Add    ( orangeColorItem   );
            this.colorItem.MenuItems.Add    ( yellowColorItem   );
            this.colorItem.MenuItems.Add    ( blueColorItem     );
            this.colorItem.MenuItems.Add    ( purpleColorItem   );

            this.objectSelect.Click     += new System.EventHandler( this.SetToObjectMode        );
            this.vertexSelect.Click     += new System.EventHandler( this.SetToVertexMode        );
            this.moveItem.Click         += new System.EventHandler( this.SetToMoveMode          );
            this.rotateItem.Click       += new System.EventHandler( this.SetToRotateMode        );
            this.scaleItem.Click        += new System.EventHandler( this.SetToScaleMode         );
            this.deleteItem.Click       += new System.EventHandler( this.DeleteEvent            );
            this.clearItem.Click        += new System.EventHandler( this.ClearWorkbase            );
            this.extendVertexItem.Click += new System.EventHandler( this.SetToExtendVertexMode  );
            this.drawVertexItem.Click   += new System.EventHandler( this.SetToDrawVertexMode    );
            
            newItem.Click               += new System.EventHandler( this.NewWorkbase              );
            openItem.Click              += new System.EventHandler( this.LoadWorkbase             );
            saveItem.Click              += new System.EventHandler( this.SaveWorkbaseEvent        );
            saveAsItem.Click            += new System.EventHandler( this.SaveAsWorkbaseEvent      );
            exitItem.Click              += new System.EventHandler( this.Exit                   );

            triangleItem.Click          += new System.EventHandler( this.SetToCreateTriangle    );
            squareItem.Click            += new System.EventHandler( this.SetToCreateSquare      );
            circleItem.Click            += new System.EventHandler( this.SetToCreateEllipse     );
            
            blackColorItem.Click        += new System.EventHandler( this.SetToBlackColor        );
            grayColorItem.Click         += new System.EventHandler( this.SetToGrayColor         );
            brownColorItem.Click        += new System.EventHandler( this.SetToBrownColor        );
            redColorItem.Click          += new System.EventHandler( this.SetToRedColor          );
            pinkColorItem.Click         += new System.EventHandler( this.SetToPinkColor         );
            greenColorItem.Click        += new System.EventHandler( this.SetToGreenColor        );
            orangeColorItem.Click       += new System.EventHandler( this.SetToOrangeColor       );
            yellowColorItem.Click       += new System.EventHandler( this.SetToYellowColor       );
            blueColorItem.Click         += new System.EventHandler( this.SetToBlueColor         );
            purpleColorItem.Click       += new System.EventHandler( this.SetToPurpleColor       );

            PopulateMenu();
        }
               

        private void PopulateMenu()
        {
            var reflectionField = typeof( MainMenu ).GetField( "form" , BindingFlags.NonPublic | BindingFlags.Instance );
            reflectionField.SetValue( this.Menu , null );

            this.Menu.MenuItems.Clear();
            
            this.Menu.MenuItems.Add ( fileItem      );
            this.Menu.MenuItems.Add ( createItem    );
            this.Menu.MenuItems.Add ( submodeItem   );
            this.Menu.MenuItems.Add ( moveItem      );

            if( this.EditSubmode == Submode.Vertex )
            {
                this.Menu.MenuItems.Add ( extendVertexItem  );
            }
            else
            {
                this.Menu.MenuItems.Add ( rotateItem        );
                this.Menu.MenuItems.Add ( scaleItem         );
            }

            this.Menu.MenuItems.Add ( deleteItem        );
            this.Menu.MenuItems.Add ( drawVertexItem    );
            this.Menu.MenuItems.Add ( clearItem         );

            reflectionField.SetValue( this.Menu , this );
            this.Menu.MenuItems.Add( colorItem );
            this.Invalidate();
        }


        private void SetBrushColor( Color color )
        {
            if( this.SelectedIndex != INVALID_INDEX )
            {
                Shape shape = this.Shapes[this.SelectedIndex];
                shape.Color = color;
                shape.Invalidate();

                this.RenderScreen();
            }

            this.SelectedColor = color;
        }

        private void SetToCreateTriangle    ( object sender , EventArgs e ) { this.EditMode = Mode.Create;  this.EditSubmode = Submode.Object;  this.ObjectType = typeof( Triangle  );  this.CreationObject = null; this.SelectedIndex = INVALID_INDEX; this.PopulateMenu();    }
        private void SetToCreateSquare      ( object sender , EventArgs e ) { this.EditMode = Mode.Create;  this.EditSubmode = Submode.Object;  this.ObjectType = typeof( Square    );  this.CreationObject = null; this.SelectedIndex = INVALID_INDEX; this.PopulateMenu();    }
        private void SetToCreateEllipse     ( object sender , EventArgs e ) { this.EditMode = Mode.Create;  this.EditSubmode = Submode.Object;  this.ObjectType = typeof(Circle);  this.CreationObject = null; this.SelectedIndex = INVALID_INDEX; this.PopulateMenu();    }
        
        private void SetToObjectMode        ( object sender , EventArgs e )
        {
            this.EditSubmode = Submode.Object;
            this.ObjectType = null;
            this.CreationObject = null;
            this.PopulateMenu();

            switch( this.EditMode )
            {
                case Mode.Draw:
                case Mode.Extend:
                    this.EditMode = Mode.Select;
                    break;
            }
        }

        private void SetToVertexMode( object sender , EventArgs e )
        {
            this.EditSubmode = Submode.Vertex;
            this.ObjectType = null;
            this.CreationObject = null;
            this.PopulateMenu();

            switch( this.EditMode )
            {
                case Mode.Rotate:
                case Mode.Scale:
                    this.EditMode = Mode.Select;
                    break;
            }
        }
        
        private void ClearWorkbase            ( object sender , EventArgs e ) { this.Shapes.Clear();  this.CancelOngoingOperation();                                  } // Cancel any ongoing operation and reset.
        private void SetToSelectMode        ( object sender , EventArgs e ) { this.EditMode = Mode.Select;  this.PopulateMenu();                                    }
        private void SetToMoveMode          ( object sender , EventArgs e ) { this.EditMode = Mode.Move;    this.PopulateMenu();                                    }
        private void SetToRotateMode        ( object sender , EventArgs e ) { this.EditMode = Mode.Rotate;  this.EditSubmode = Submode.Object; this.PopulateMenu(); }
        private void SetToScaleMode         ( object sender , EventArgs e ) { this.EditMode = Mode.Scale;   this.EditSubmode = Submode.Object; this.PopulateMenu(); }
        private void SetToExtendVertexMode  ( object sender , EventArgs e ) { this.EditMode = Mode.Extend;  this.EditSubmode = Submode.Vertex; this.PopulateMenu(); }
        private void SetToDrawVertexMode    ( object sender , EventArgs e ) { this.EditMode = Mode.Draw;    this.EditSubmode = Submode.Vertex; this.PopulateMenu(); }
        
        private void DeleteEvent            ( object sender , EventArgs e )
        {
            if( this.SelectedIndex != INVALID_INDEX )
            {
                Shape shape = this.Shapes[this.SelectedIndex];

                if( this.EditSubmode == Submode.Vertex )
                {
                    ( ( NGon ) shape ).DeleteVertex( this.SelectedVertexId );
                    shape.Invalidate();

                    this.SelectedVertexId   = INVALID_INDEX;
                    this.EditMode           = Mode.Select;
                }
                
                if( this.EditSubmode == Submode.Object || !shape.IsValid )
                {
                    this.DeleteSelectedObject();
                }

                this.RenderScreen();
            }
        }



        private void SaveToFile()
        {
            IFormatter formatter    = new BinaryFormatter();
            Stream stream           = new FileStream( this.Filename , FileMode.OpenOrCreate , FileAccess.Write );
            formatter.Serialize( stream , this.Shapes );
            stream.Close();
        }



        private void LoadFromFile()
        {
            IFormatter formatter    = new BinaryFormatter();
            Stream stream           = new FileStream( this.Filename , FileMode.Open , FileAccess.Read );
            this.Shapes             = ( List<Shape> ) formatter.Deserialize( stream );
            stream.Close();

            this.RenderScreen();
        }



        private void NewWorkbase( object sender , EventArgs e )
        {
            DialogResult dialogResult = MessageBox.Show( "Do you want to save your current Workbase?" , "Confirm New" , MessageBoxButtons.YesNo );

            if( dialogResult == DialogResult.Yes )
            {
                if( !this.SaveWorkbase() )
                {
                    return;
                }
            }

            this.ClearWorkbase( null , null );
            this.Filename = null;
        }



        private void LoadWorkbase( object sender , EventArgs e )
        {
            this.FileDialog.ReadOnlyChecked = true;
            this.FileDialog.CheckFileExists = true;

            if( this.FileDialog.ShowDialog() == DialogResult.OK )
            {
                this.Filename = this.FileDialog.FileName;
                this.LoadFromFile();
            }
        }



        private bool SaveWorkbase()
        {
            if( this.Filename == null )
            {
                return this.SaveAsWorkbase();
            }
            else
            {
                this.SaveToFile();
                return true;
            }
        }



        private void SaveWorkbaseEvent( object sender , EventArgs e )
        {
            this.SaveWorkbase();
        }



        private bool SaveAsWorkbase()
        {
            this.FileDialog.ReadOnlyChecked = false;
            this.FileDialog.CheckFileExists = false;

            while( this.FileDialog.ShowDialog() == DialogResult.OK )
            {
                if( File.Exists( this.FileDialog.FileName ) )
                {
                    DialogResult dialogResult = MessageBox.Show( "Are you sure you want to overwrite an existing file?" , "Confirm Overwrite" , MessageBoxButtons.YesNo );

                    if( dialogResult == DialogResult.No )
                    {
                        continue;
                    }
                }

                this.Filename = this.FileDialog.FileName;
                this.SaveToFile();
                return true;
            }

            return false;
        }



        private void SaveAsWorkbaseEvent( object sender , EventArgs e )
        {
            this.SaveAsWorkbase();
        }



        private void Exit( object sender , EventArgs e )
        {
            Application.Exit();
        }



        private void OnFormClosing( object sender , FormClosingEventArgs e )
        {
            DialogResult dialogResult = MessageBox.Show( "Do you want to save your current Workbase?" , "Confirm Exit" , MessageBoxButtons.YesNo );

            if( dialogResult == DialogResult.Yes )
            {
                if( !this.SaveWorkbase() )
                {
                    e.Cancel = true;
                }
            }
        }



        private void SetToBlackColor        ( object sender , EventArgs e ) { this.SetBrushColor( Color.Black   ); }
        private void SetToGrayColor         ( object sender , EventArgs e ) { this.SetBrushColor( Color.Gray    ); }
        private void SetToBrownColor        ( object sender , EventArgs e ) { this.SetBrushColor( Color.Brown   ); }
        private void SetToRedColor          ( object sender , EventArgs e ) { this.SetBrushColor( Color.Red     ); }
        private void SetToPinkColor         ( object sender , EventArgs e ) { this.SetBrushColor( Color.Pink    ); }
        private void SetToGreenColor        ( object sender , EventArgs e ) { this.SetBrushColor( Color.Green   ); }
        private void SetToOrangeColor       ( object sender , EventArgs e ) { this.SetBrushColor( Color.Orange  ); }
        private void SetToYellowColor       ( object sender , EventArgs e ) { this.SetBrushColor( Color.Yellow  ); }
        private void SetToBlueColor         ( object sender , EventArgs e ) { this.SetBrushColor( Color.Blue    ); }
        private void SetToPurpleColor       ( object sender , EventArgs e ) { this.SetBrushColor( Color.Purple  ); }



        private void RenderScreen()
        {
            this.RenderTarget.Dispose();
            this.RenderTarget = new Bitmap( this.Width , this.Height );
            
            using( Graphics rtGraphics = Graphics.FromImage( this.RenderTarget ) )
            {
                rtGraphics.Clear( this.BackColor );
                
                // Draw existing object
                foreach( Shape shape in this.Shapes )
                {
                    shape.SetScreenSize( this.Width , this.Height );
                    shape.DrawImage( Color.Red );
                    rtGraphics.DrawImage( shape.RasterImage , 0 , 0 );
                }
               
                if( this.CreationObject != null )
                {
                    this.CreationObject.SetScreenSize( this.Width , this.Height );
                    this.CreationObject.DrawImage( Color.Red );
                    rtGraphics.DrawImage( this.CreationObject.RasterImage , 0 , 0 );
                }

                if( this.SelectedIndex != INVALID_INDEX )
                {
                    if( this.EditSubmode == Submode.Object )
                    {
                        Shape shape = this.Shapes[this.SelectedIndex];
                        Bitmap mask = shape.EdgeSelectionMask;
                        rtGraphics.DrawImage( mask , 0 , 0 );
                    }
                    else if( this.EditSubmode == Submode.Vertex && this.SelectedVertexId != INVALID_INDEX )
                    {
                        List<Point> selectedVertexList = new List<Point>();
                        selectedVertexList.Add( this.SelectedVertex );

                        RasterTools.FillPoints( rtGraphics , Color.Red , selectedVertexList , VertexSelectionHighlight );
                    }
                }

                RasterTools.FillPoints( rtGraphics , Color.Red , this.DrawLine );

                Font font           = new Font( "Arial" , 9 );
                Brush brush         = new SolidBrush( Color.Black );
                string modeString   = this.EditMode.ToString();
                string modeAddon    = this.EditSubmode.ToString();

                if( this.EditMode == Mode.Create )
                {
                    string[] objectNames    = this.ObjectType.ToString().Split('.');
                    modeAddon               = objectNames[objectNames.Length - 1];
                }

                rtGraphics.DrawString( modeString + " " + modeAddon , font , brush , this.MouseCurrent.X + 10 , this.MouseCurrent.Y + 20 );
            }

            this.Graphics.DrawImage( this.RenderTarget , 0 , 0 );
        }



        private void RasterizeDrawingLine()
        {
            if( this.IsSelectionClicked )
            {
                RasterTools.RasterizeLine( this.DrawLine , this.SelectedVertex.X , this.SelectedVertex.Y , this.MouseCurrent.X , this.MouseCurrent.Y );
            }
            else if( this.EditMode == Mode.Draw )
            {
                RasterTools.RasterizeLine( this.DrawLine , this.MouseLastClicked.X , this.MouseLastClicked.Y , this.MouseCurrent.X , this.MouseCurrent.Y );
            }
        }



        protected override void OnPaint( PaintEventArgs e )
        {
            base.OnPaint( e );
            this.RenderScreen();
        }



        // Mouse Control
        private void OnMouseDown( object sender , MouseEventArgs e )
        {
            if( e.Button == MouseButtons.Left )
            {
                this.MouseLastClicked.X = e.X;
                this.MouseLastClicked.Y = e.Y;

                if( this.IsMouseDown && this.EditMode == Mode.Draw )
                    return;

                this.IsMouseDown = true;
                
                if( this.EditMode == Mode.Create )
                {
                    if( this.CreationObject == null )
                    {
                        this.CreationObject = ( Shape ) Activator.CreateInstance( this.ObjectType , this.SelectedColor );
                    }
                }
                else
                {
                    this.SelectObjectUnderCursor();
                }

                if( this.SelectedIndex != INVALID_INDEX )
                {
                    Shape shape = this.Shapes[this.SelectedIndex];

                    if( this.EditSubmode == Submode.Vertex && typeof( NGon ).IsAssignableFrom( shape.GetType() ) )
                    {
                        int vertexId;
                        Point vertex;

                        double closestDistance  = ( ( NGon ) shape ).GetClosestVertex( this.MouseLastClicked , SELECT_VERTEX_THRESHOLD , out vertexId , out vertex );
                        this.IsSelectionClicked = ( closestDistance < SELECT_VERTEX_THRESHOLD && vertexId == this.SelectedVertexId );
                    }
                    else
                    {
                        this.IsSelectionClicked = shape.IsEdge( this.MouseLastClicked );
                    }
                }
                else
                {
                    this.IsSelectionClicked = false;
                }
            }
        }



        private void OnMouseMoved( object sender , MouseEventArgs e )
        {
            this.MousePrevious  = this.MouseCurrent;
            this.MouseCurrent.X = e.X;
            this.MouseCurrent.Y = e.Y;
            this.MouseMoved     = this.MouseCurrent.X != this.MousePrevious.X || this.MouseCurrent.Y != this.MousePrevious.Y;

            if( this.IsMouseDown && this.MouseMoved )
            {
                if( this.EditMode == Mode.Create && this.CreationObject != null )
                {
                    // Modify the working object
                    this.CreationObject.Invalidate();
                    this.CreationObject.SetRegion( this.MouseLastClicked , this.MouseCurrent );
                    this.CreationObject.Rasterize();
                }
                else if( this.EditMode == Mode.Move && this.IsSelectionClicked )
                {
                    Shape shape         = this.Shapes[this.SelectedIndex];
                    Point mouseDelta    = new Point( this.MouseCurrent.X - this.MousePrevious.X , this.MouseCurrent.Y - this.MousePrevious.Y );

                    if ( this.EditSubmode == Submode.Object )
                    {
                        shape.Move( mouseDelta );
                    }
                    else if( this.EditSubmode == Submode.Vertex )
                    {
                        this.SelectedVertex = ( ( NGon ) shape ).MoveVertex( this.SelectedVertexId , mouseDelta );
                    }

                    shape.Invalidate();
                }
                else if( this.EditMode == Mode.Rotate && this.SelectedIndex != INVALID_INDEX )
                {
                    Shape shape         = this.Shapes[this.SelectedIndex];
                    PointF mouseLast    = new PointF( this.MousePrevious.X   - shape.Origin.X , this.MousePrevious.Y    - shape.Origin.Y );
                    PointF mouseNow     = new PointF( this.MouseCurrent.X    - shape.Origin.X , this.MouseCurrent.Y     - shape.Origin.Y );

                    double slopeLast    = mouseLast.Y / mouseLast.X;
                    double slopeNow     = mouseNow.Y / mouseNow.X;
                    double angleDelta   = Math.Atan2( slopeLast - slopeNow , 1 + ( slopeLast * slopeNow ) ) * RTOD;

                    // Correct axis
                    if( Math.Sign( mouseLast.X ) != Math.Sign( mouseNow.X ) )
                    {
                        angleDelta += slopeLast < slopeNow ? -180 : 180;
                    }

                    if ( this.EditSubmode == Submode.Object )
                    {
                        shape.Rotate( angleDelta );
                    }

                    shape.Invalidate();
                }
                else if( this.EditMode == Mode.Scale && this.SelectedIndex != INVALID_INDEX )
                {
                    Shape shape         = this.Shapes[this.SelectedIndex];
                    PointF mouseLast    = new PointF( this.MousePrevious.X   - shape.Origin.X , this.MousePrevious.Y    - shape.Origin.Y );
                    PointF mouseNow     = new PointF( this.MouseCurrent.X    - shape.Origin.X , this.MouseCurrent.Y     - shape.Origin.Y );

                    // Calculate the scale difference
                    double xScale = mouseNow.X / mouseLast.X;
                    double yScale = mouseNow.Y / mouseLast.Y;

                    if ( this.EditSubmode == Submode.Object )
                    {
                        shape.Resize( xScale , yScale );
                    }

                    shape.Invalidate();
                }
                else if( this.EditMode == Mode.Extend || this.EditMode == Mode.Draw || this.EditMode == Mode.Draw )
                {
                    this.DrawLine.Clear();
                    this.RasterizeDrawingLine();
                }
            }

            // Refresh screen
            this.RenderScreen();
        }



        private void OnMouseUp( object sender , MouseEventArgs e )
        {
            if( e.Button == MouseButtons.Left && this.IsMouseDown )
            {
                this.IsMouseDown = false;
                this.DrawLine.Clear();

                if( this.EditMode == Mode.Create )
                {
                    if( this.CreationObject != null && this.CreationObject.IsValid )
                    {
                        this.SelectedIndex = this.Shapes.Count;

                        this.CreationObject.CommitScale();
                        this.CreationObject.Invalidate();
                        this.Shapes.Add( this.CreationObject );
                        this.RenderScreen();
                        this.SetToMoveMode( null , null );
                    }

                    this.CreationObject = null;
                }
                else if( this.EditMode == Mode.Select )
                {
                    this.SelectObjectUnderCursor();
                }
                
                else if( this.EditMode == Mode.Extend || this.EditMode == Mode.Draw || this.EditMode == Mode.Draw )
                {
                    if( this.SelectedIndex != INVALID_INDEX && this.SelectedVertexId != INVALID_INDEX )
                    {
                        Point otherVertex;
                        int otherVertexId;
                        int otherSelection  = GetObjectUnderCursor( out otherVertex , out otherVertexId );
                        Shape firstShape    = this.Shapes[this.SelectedIndex];

                        if( typeof( NGon ).IsAssignableFrom( firstShape.GetType() ) )
                        {
                            NGon nGonSelection  = ( NGon ) firstShape;
                            bool doneOperation  = false;

                            if( otherSelection != INVALID_INDEX && otherVertexId != INVALID_INDEX )
                            {
                                Shape otherShape = this.Shapes[otherSelection];

                                if( typeof( NGon ).IsAssignableFrom( otherShape.GetType() ) )
                                {
                                    int lastSelectedVertex  = this.SelectedVertexId;
                                    NGon nGonOther          = ( NGon ) otherShape;
                                    doneOperation           = true;

                                    if( this.EditMode == Mode.Draw )
                                    {
                                        //Edit Mode Draw
                                    }
                                    else
                                    {
                                        this.SelectedVertex = nGonSelection.LinkVertex( this.SelectedVertexId , otherVertexId , nGonOther , out this.SelectedVertexId );
                                    }

                                    nGonSelection.Invalidate();

                                    if( firstShape != otherShape )
                                    {
                                        this.DeleteObject( otherSelection );
                                    }
                                }
                            }

                            if( ( this.EditMode == Mode.Extend || this.EditMode == Mode.Draw ) && !doneOperation )
                            {
                                this.SelectedVertexId   = nGonSelection.AddVertex( this.MouseCurrent , this.SelectedVertexId );
                                this.SelectedVertex     = this.MouseCurrent;
                                this.IsMouseDown        = this.EditMode == Mode.Draw;

                                nGonSelection.Invalidate();
                                this.RasterizeDrawingLine();
                            }
                        }

                        this.ValidateSelectedObject();
                    }

                    else if( this.EditMode == Mode.Draw )
                    {
                        // Generate a new polygon
                        Polygon polygon = new Polygon( this.SelectedColor , this.MouseLastClicked , this.MouseCurrent );
                        polygon.Invalidate();

                        this.SelectedIndex      = this.Shapes.Count;
                        this.SelectedVertex     = this.MouseCurrent;
                        this.SelectedVertexId   = polygon.Vertices.Count - 1;
                        this.IsMouseDown        = true;

                        this.Shapes.Add( polygon );
                        this.RasterizeDrawingLine();
                    }
                }

                this.MouseLastClicked = this.MouseCurrent;
            }
        }



        private void ValidateSelectedObject()
        {
            if( this.SelectedIndex != INVALID_INDEX )
            {
                Shape shape = this.Shapes[this.SelectedIndex];

                if( !shape.IsValid )
                {
                    this.DeleteSelectedObject();
                }
                else if( this.SelectedVertexId != INVALID_INDEX && this.SelectedVertexId > ( ( NGon ) shape ).Vertices.Count )
                {
                    this.SelectedVertexId = INVALID_INDEX;
                }
            }
        }



        private void DeleteSelectedObject()
        {
            this.DeleteObject( this.SelectedIndex );
        }



        private void DeleteObject( int objectId )
        {
            this.Shapes.RemoveAt( objectId );

            if( this.SelectedIndex > objectId )
            {
                this.SelectedIndex--;
            }
            else if( this.SelectedIndex == objectId )
            {
                this.SelectedIndex = INVALID_INDEX;
            }
        }



        private void SelectObjectUnderCursor()
        {
            Point selectedVertex;
            int selectedVertexId;
            int currentSelection = GetObjectUnderCursor( out selectedVertex , out selectedVertexId );

            if( this.SelectedIndex != currentSelection )
            {
                this.SelectedVertexId = INVALID_INDEX;
            }

            if( currentSelection != INVALID_INDEX && this.EditSubmode == Submode.Vertex )
            {
                this.SelectedVertex     = selectedVertex;
                this.SelectedVertexId   = selectedVertexId;
            }

            this.SelectedIndex = currentSelection;
            this.PopulateMenu();
        }



        private int GetObjectUnderCursor( out Point selectionVertex , out int selectionVertexId )
        {
            int selectionIndex      = INVALID_INDEX;
            selectionVertexId       = INVALID_INDEX;
            selectionVertex         = new Point( 0 , 0 );
            double shortestDistance = SELECT_VERTEX_THRESHOLD;

            for( int i = this.Shapes.Count - 1 ; i >= 0 ; i-- )
            {
                if( this.EditSubmode == Submode.Vertex )
                {
                    int closestVertexId;
                    Point closestVertex;
                    double newShortestDistance = this.Shapes[i].GetClosestVertex( this.MouseCurrent , shortestDistance , out closestVertexId , out closestVertex );

                    if( newShortestDistance < shortestDistance )
                    {
                        selectionVertex     = closestVertex;
                        selectionVertexId   = closestVertexId;
                        shortestDistance    = newShortestDistance;
                        selectionIndex      = i;
                    }
                }
                else if( this.Shapes[i].IsEdge( this.MouseCurrent ) )
                {
                    selectionIndex = i;
                    break;
                }
            }

            return selectionIndex;
        }



        // Keyboard Methods
        private void OnKeyUp( object sender , KeyEventArgs e )
        {
            switch( e.KeyCode )
            {
                case( Keys.D1       ): this.SetToCreateTriangle     ( null , null ); break;
                case( Keys.D2       ): this.SetToCreateSquare       ( null , null ); break;
                case( Keys.Q        ): this.SetToMoveMode           ( null , null ); break;
                case( Keys.W        ): this.SetToRotateMode         ( null , null ); break;
                case( Keys.E        ): this.SetToScaleMode          ( null , null ); break;
                case( Keys.Z        ): this.SetToObjectMode         ( null , null ); break;
                case( Keys.X        ): this.SetToVertexMode         ( null , null ); break;
                case( Keys.C        ): this.SetToDrawVertexMode     ( null , null ); break;
                case( Keys.Back     ): this.ClearWorkbase             ( null , null ); break;
                case( Keys.Delete   ): this.DeleteEvent             ( null , null ); break;
                case( Keys.Escape   ):
                    this.CancelOngoingOperation();
                    break;
               
                // Special Keys
                case( Keys.S ):
                    if( ModifierKeys.HasFlag( Keys.Control ) )
                    {
                        this.SaveWorkbase();
                    }
                    {
                        this.SetToExtendVertexMode( null , null );
                    }
                    break;
               
                case( Keys.N ):
                    if( ModifierKeys.HasFlag( Keys.Control ) )
                    {
                        this.NewWorkbase( null , null );
                    }
                    break;
               
                case( Keys.O ):
                    if( ModifierKeys.HasFlag( Keys.Control ) )
                    {
                        this.LoadWorkbase( null , null );
                    }
                    break;
            }
        }

        private void CancelOngoingOperation()
        {
            this.SelectedIndex      = INVALID_INDEX;
            this.IsMouseDown        = false;
            this.IsSelectionClicked = false;
            this.CreationObject     = null;
            this.DrawLine.Clear();
            this.RenderScreen();
        }

        private void GrafPack_Load(object sender, EventArgs e)
        {

        }
    }
}


