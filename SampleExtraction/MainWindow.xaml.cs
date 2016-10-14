using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SampleExtraction
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public static string imageDIR = System.AppDomain.CurrentDomain.BaseDirectory;

        public static string saveImageDIR = "";

        Image<Bgra, Byte> image;
        //BitmapImage bitmapImage;

        System.Windows.Point mousePoint;

        System.Windows.Point startPoint;

        StringBuilder resultString;

        bool flag_MouseDown = false;

        List<System.Drawing.Rectangle> autoFacesCheck;

        List<System.Drawing.Rectangle> totalFaceBoxes;

        public static List<string> imagePathList;

        public DataUpdate dataUpdate;

        string imagePath_NowLoaded;

        BitmapSource _bitmapSource;
        public BitmapSource bitmapSource
        {
            get { return _bitmapSource; }

            set
            {
                if (_bitmapSource != value)
                {
                    _bitmapSource = null;
                    _bitmapSource = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("bitmapSource"));
                }
            }
        }
        //BitmapSource _boudingBoxBitmapSource;
        //public BitmapSource boudingBoxBitmapSource
        //{
        //    get { return _boudingBoxBitmapSource; }

        //    set
        //    {
        //        if (_boudingBoxBitmapSource != value)
        //        {
        //            _boudingBoxBitmapSource = value;
        //            if (PropertyChanged != null)
        //                PropertyChanged(this, new PropertyChangedEventArgs("boudingBoxBitmapSource"));
        //        }
        //    }
        //}

        public MainWindow()
        {
            InitializeComponent();
            resultString = new StringBuilder();
            imagePathList = new List<string>();
            dataUpdate = new DataUpdate();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //CvInvoke.UseOpenCL = false;
            imageShow.DataContext = this;
            textBlock_Width.DataContext = dataUpdate;
            textBlock_Height.DataContext = dataUpdate;
            textBlock_ImageCount.DataContext = dataUpdate;
            textBlock_FinishedCount.DataContext = dataUpdate;
            textBlock_autoCheckTime.DataContext = dataUpdate;
            textBlock_autoCheckFaceCount.DataContext = dataUpdate;
            //image_BoudingBox.DataContext = this;
            Task task = Task.Factory.StartNew(() => {
                getImagePath(imageDIR,ref imagePathList);
            });
            task.ContinueWith(t =>
            {
                dataUpdate.imageCount = imagePathList.Count;
            });

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (image != null)
                {
                    if ((bool)checkBox_Neg.IsChecked)
                    {
                        //Task T = Task.Factory.StartNew(() => {
                            

                        //});
                        cutImage();
                    }
                    else
                    {
                        if ((bool)radioButton_Neg.IsChecked)
                        {
                            writeImageNeg(resultString.ToString());
                        }
                        else
                        {
                            writeImageInfo(resultString.ToString());
                        }
                    }
                    image.Dispose();
                    image = null;
                    resultString.Clear();
                    dataUpdate.finishedCount++;
                    border_BoudingBox.Margin = new Thickness(0, 0, 0, 0);
                    image_BoudingBox.Width = 0;
                    image_BoudingBox.Height = 0;
                }

                if (totalFaceBoxes != null)
                {
                    totalFaceBoxes.Clear();
                }
                else
                {
                    totalFaceBoxes = new List<System.Drawing.Rectangle>();
                }

                if (!textBox_Jump.Text.Equals(""))
                {
                    int jumpNum = Convert.ToInt32(textBox_Jump.Text);
                    imagePathList.RemoveRange(0, jumpNum - 1);
                    textBox_Jump.Text = "";
                    dataUpdate.finishedCount += jumpNum - 1;
                }

                if (imagePathList.Count != 0)
                {
                    imagePath_NowLoaded = imagePathList.First();
                    imagePathList.RemoveAt(0);
                }
                else
                {
                    dataUpdate.finishedCount--;
                    MessageBox.Show("无可标记图片", "错误");
                    return;
                }


                image = new Image<Bgra, byte>(imagePath_NowLoaded);
                
                //bitmapImage = new BitmapImage(new Uri(imagePath_NowLoaded));

                if (image.Width > 5000 || image.Height > 5000)
                {
                    image.Dispose();
                    image = null;
                    button_Click(this, e);
                }

                imageShow.Width = image.Width;
                imageShow.Height = image.Height;
                dataUpdate.imageWidth = image.Width;
                dataUpdate.imageHeight = image.Height;
                
                if ((bool)checkBox_AutoBounding.IsChecked)
                {
                    //long checkTime = 0;
                    //autoFacesCheck = new List<System.Drawing.Rectangle>();
                    //DetectFace.Detect(image.Mat,"face_default.xml",autoFacesCheck,false,out checkTime);
                    //dataUpdate.autoCheckTime = checkTime;
                    //foreach (System.Drawing.Rectangle face in autoFacesCheck)
                    //{
                    //    CvInvoke.Rectangle(image, face, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
                    //    totalFaceBoxes.Add(face);
                    //}

                    Task T = Task.Factory.StartNew(() => {
                        autoCheckFaceThread();
                    });
                }else
                {
                    Bitmap tempBitmap = image.Bitmap;
                    bitmapSource = CreateBitmapSourceFromBitmap(ref tempBitmap);
                    tempBitmap.Dispose();
                    tempBitmap = null;
                }

                resultString.Append(imagePath_NowLoaded);

                canvas_BoudingBox.Margin = new Thickness(0,-dataUpdate.imageHeight - 6,0,0);

            }catch(Exception Ex)
            {
                imagePathList.RemoveAt(0);
                MessageBox.Show("报错了！","Warning");
            }finally
            {
                radioButton_Neg.IsChecked = false;
                radioButton_Pos.IsChecked = true;
            }

            //string name = "testWindows";
            //CvInvoke.NamedWindow(name);

            //using (Image<Bgr, Byte> image = new Image<Bgr, byte>("1.jpg"))
            //{
            //    CvInvoke.Imshow(name, image);
            //    CvInvoke.WaitKey(0);
            //    CvInvoke.DestroyWindow(name);

            //}
            //Mat img = new Mat(200,200,Emgu.CV.CvEnum.DepthType.Cv8U,3);
            //img.SetTo(new Bgr(255,0,0).MCvScalar);
            //CvInvoke.PutText(
            //    img,
            //    "Hello",
            //    new System.Drawing.Point(10,80),
            //    FontFace.HersheyComplex,
            //    1.0,
            //    new Bgr(0,255,0).MCvScalar);


        }

        private void autoCheckFaceThread()
        {
            long checkTime = 0;
            autoFacesCheck = new List<System.Drawing.Rectangle>();
            DetectFace.Detect(image.Mat, "face_default.xml", autoFacesCheck, false, out checkTime);
            dataUpdate.autoCheckTime = checkTime;
            dataUpdate.faceCheckSuccessed = autoFacesCheck.Count();
            foreach (System.Drawing.Rectangle face in autoFacesCheck)
            {
                CvInvoke.Rectangle(image, face, new Bgr(System.Drawing.Color.Red).MCvScalar, 2);
                totalFaceBoxes.Add(face);
            }
            callDisplayImage_Delegate();
        }

        private delegate void displayImage_Delegate();

        private void callDisplayImage_Delegate()
        {
            this.Dispatcher.Invoke(new displayImage_Delegate(displayImage));
        }

        private void displayImage()
        {
            Bitmap tempBitmap = image.Bitmap;
            bitmapSource = CreateBitmapSourceFromBitmap(ref tempBitmap);
            tempBitmap.Dispose();
            tempBitmap = null;
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static BitmapSource CreateBitmapSourceFromBitmap(ref Bitmap bitmap)
        {
            IntPtr ptr = bitmap.GetHbitmap();
            BitmapSource result =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            //release resource  
            DeleteObject(ptr);

            return result;
        }

        //public static BitmapSource CreateBitmapSourceFromBitmap(ref Bitmap bitmap)
        //{
        //    if (bitmap == null)
        //        throw new ArgumentNullException("bitmap");
        //    return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
        //    bitmap.GetHbitmap(),
        //    IntPtr.Zero,
        //    Int32Rect.Empty,
        //    BitmapSizeOptions.FromEmptyOptions());
        //}

        private void imageShow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(imageShow);
            resultString.Append(" " + (int)startPoint.X + " " + (int)startPoint.Y + " ");
            flag_MouseDown = true;
            
        }

        private void imageShow_MouseMove(object sender, MouseEventArgs e)
        {
            if (flag_MouseDown)
            {
                mousePoint = e.GetPosition(imageShow);
                //CvInvoke.Rectangle(image,new System.Drawing.Rectangle((int)startPoint.X, (int)startPoint.Y, (int)(mousePoint.X - startPoint.X), (int)(mousePoint.Y - startPoint.Y)),new Rgba(0,255,0,20).MCvScalar,2);
                //bitmapSource = CreateBitmapSourceFromBitmap(image.Bitmap);
                //(mousePoint);
                label_Position.Content = "X:" + mousePoint.X + "Y;" + mousePoint.Y;

                border_BoudingBox.Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0);
                if (startPoint.X <= mousePoint.X)
                {
                    image_BoudingBox.Width = mousePoint.X - startPoint.X;
                }else
                {
                    image_BoudingBox.Width = startPoint.X - mousePoint.X;
                }
                if (startPoint.Y <= mousePoint.Y)
                {
                    image_BoudingBox.Height = mousePoint.Y - startPoint.Y;
                }else
                {
                    image_BoudingBox.Height = startPoint.Y - mousePoint.Y;
                }
                
            }
        }

        private void imageShow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resultString.Append((int)(mousePoint.X - startPoint.X) + " " + (int)(mousePoint.Y - startPoint.Y));
            flag_MouseDown = false;
            label_Position.Content = resultString.ToString();

            totalFaceBoxes.Add(new System.Drawing.Rectangle((int)startPoint.X, (int)startPoint.Y, (int)(mousePoint.X - startPoint.X), (int)(mousePoint.Y - startPoint.Y)));
            //border_BoudingBox.Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0);
            //if (startPoint.X <= mousePoint.X)
            //{
            //    image_BoudingBox.Width = mousePoint.X - startPoint.X;
            //}
            //else
            //{
            //    image_BoudingBox.Width = startPoint.X - mousePoint.X;
            //}
            //if (startPoint.Y <= mousePoint.Y)
            //{
            //    image_BoudingBox.Height = mousePoint.Y - startPoint.Y;
            //}
            //else
            //{
            //    image_BoudingBox.Height = startPoint.Y - mousePoint.Y;
            //}
            if (!(bool)checkBox_Neg.IsChecked)
            {
                CvInvoke.Rectangle(image, new System.Drawing.Rectangle((int)startPoint.X, (int)startPoint.Y, (int)(mousePoint.X - startPoint.X), (int)(mousePoint.Y - startPoint.Y)), new Rgb(System.Drawing.Color.Azure).MCvScalar, 2);
                Bitmap tempBitmap = image.Bitmap;
                try
                {
                    bitmapSource = CreateBitmapSourceFromBitmap(ref tempBitmap);
                }
                catch (Exception Ex2)
                {
                    imagePathList.RemoveAt(0);
                    MessageBox.Show("报错了！", "Warning");
                }

                tempBitmap.Dispose();
                tempBitmap = null;
            }

        }

        private void border_BoudingBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            resultString.Append((int)(mousePoint.X - startPoint.X) + " " + (int)(mousePoint.Y - startPoint.Y));
            flag_MouseDown = false;
            label_Position.Content = resultString.ToString();
            totalFaceBoxes.Add(new System.Drawing.Rectangle((int)startPoint.X, (int)startPoint.Y, (int)(mousePoint.X - startPoint.X), (int)(mousePoint.Y - startPoint.Y)));
            //border_BoudingBox.Margin = new Thickness(startPoint.X, startPoint.Y, 0, 0);
            //if (startPoint.X <= mousePoint.X)
            //{
            //    image_BoudingBox.Width = mousePoint.X - startPoint.X;
            //}
            //else
            //{
            //    image_BoudingBox.Width = startPoint.X - mousePoint.X;
            //}
            //if (startPoint.Y <= mousePoint.Y)
            //{
            //    image_BoudingBox.Height = mousePoint.Y - startPoint.Y;
            //}
            //else
            //{
            //    image_BoudingBox.Height = startPoint.Y - mousePoint.Y;
            //}
            if (!(bool)checkBox_Neg.IsChecked)
            {
                CvInvoke.Rectangle(image, new System.Drawing.Rectangle((int)startPoint.X, (int)startPoint.Y, (int)(mousePoint.X - startPoint.X), (int)(mousePoint.Y - startPoint.Y)), new Rgb(System.Drawing.Color.Azure).MCvScalar, 2);
                Bitmap tempBitmap = image.Bitmap;
                try
                {
                    bitmapSource = CreateBitmapSourceFromBitmap(ref tempBitmap);
                }
                catch (Exception Ex2)
                {
                    imagePathList.RemoveAt(0);
                    MessageBox.Show("报错了！", "Warning");
                }

                tempBitmap.Dispose();
                tempBitmap = null;
            }

        }

        private void writeImageInfo(string writeStr)
        {
            //string[] tempStr = writeStr.Split(' ');
            //int totalNum = tempStr.Count();
            //int countBoundingBox = (totalNum - 1) / 4;
            //if (countBoundingBox == 0)
            //{
            //    return;
            //}

            if (totalFaceBoxes.Count == 0)
                return;

            StringBuilder SB = new StringBuilder();
            SB.Append(imagePath_NowLoaded + " " + totalFaceBoxes.Count + " ");
            foreach (System.Drawing.Rectangle face in totalFaceBoxes)
            {
                SB.Append(face.X + " " + face.Y + " " + face.Width + " " + face.Height + " ");
            }


            //for (int i = 1; i < totalNum;i++)
            //{
            //    SB.Append(tempStr[i] + " ");
            //}
            SB.Remove(SB.Length - 1, 1);
            using(StreamWriter SW = new StreamWriter(new FileStream("info.dat",FileMode.Append)))
            {
                SW.WriteLine(SB.ToString());
            }
        }

        private void writeImageNeg(string writeStr)
        {
            using(StreamWriter SW = new StreamWriter(new FileStream("neg.txt",FileMode.Append)))
            {
                SW.WriteLine(writeStr);
            }
        }

        private void getImagePath(string dir,ref List<string> pathList)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            foreach(FileInfo fileInfo in dirInfo.GetFiles("*.jpg"))
            {
                pathList.Add(fileInfo.FullName);
            }
            foreach (FileInfo fileInfo in dirInfo.GetFiles("*.png"))
            {
                pathList.Add(fileInfo.FullName);
            }
        }

        private string setImagePath()
        {
            System.Windows.Forms.FolderBrowserDialog FBD = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult DR = FBD.ShowDialog();
            if (DR == System.Windows.Forms.DialogResult.Cancel)
                return "";
            return FBD.SelectedPath;
        }

        private void imageShow_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
            resultString.Clear();
            totalFaceBoxes.Clear();
            image = new Image<Bgra, byte>(imagePath_NowLoaded);
            Bitmap tempBitmap = image.Bitmap;
            try
            {
                bitmapSource = CreateBitmapSourceFromBitmap(ref tempBitmap);
            }
            catch (Exception Ex3)
            {
                imagePathList.RemoveAt(0);
                MessageBox.Show("报错了！", "Warning");
            }
            tempBitmap.Dispose();
            tempBitmap = null;
            resultString.Append(imagePath_NowLoaded);
        }

        private void button_Path_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog FBD = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult DR = FBD.ShowDialog();
            if (DR == System.Windows.Forms.DialogResult.Cancel)
                return;
            imageDIR = FBD.SelectedPath;

            imagePathList.Clear();
            Task task = Task.Factory.StartNew(() => {
                getImagePath(imageDIR, ref imagePathList);
            });
            task.ContinueWith(t =>
            {
                dataUpdate.imageCount = imagePathList.Count;
            });
            //MessageBox.Show(imageDIR);
        }

        private void button_WritePath_Click(object sender, RoutedEventArgs e)
        {
            saveImageDIR = setImagePath();
        }

        private void cutImage()
        {
            if (!Directory.Exists(saveImageDIR))
                MessageBox.Show("存储路径有误，请重新设置！","Notice");
            try
            {
                Image<Bgra, byte> newImage = image.GetSubRect(new System.Drawing.Rectangle((int)startPoint.X, (int)startPoint.Y, (int)(mousePoint.X - startPoint.X), (int)(mousePoint.Y - startPoint.Y)));
                string saveImageName = saveImageDIR + @"\" + GetTimeStamp() + ".jpg";
                newImage.Save(saveImageName);
                newImage.Dispose();
                newImage = null;
                writeImageNeg(saveImageName);
            }catch (Exception Ex4)
            {
                MessageBox.Show("剪切图片时报错！","Warning");
            }

        }

        private string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
        //private void drawBoundingBox(System.Windows.Point point)
        //{
        //    using (Mat mat = new Mat((int)imageShow.Height, (int)imageShow.Width, DepthType.Cv8U,3))
        //    {
        //        //mat.SetTo(new Bgra(0,255,0).MCvScalar);
        //        CvInvoke.Rectangle(mat, new System.Drawing.Rectangle((int)startPoint.X, (int)startPoint.Y, (int)(mousePoint.X - startPoint.X), (int)(mousePoint.Y - startPoint.Y)), new Rgb(0, 255, 0).MCvScalar, 2);
        //        boudingBoxBitmapSource = CreateBitmapSourceFromBitmap(mat.Bitmap);
        //    }
        //}
    }
}
