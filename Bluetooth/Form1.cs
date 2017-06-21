using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using InTheHand;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Ports;
using InTheHand.Net.Sockets;
using System.IO;

namespace Bluetooth
{
    public partial class Form1 : Form
    {
        List<string> items;

        public Form1()
        {
            items = new List<string>();
            InitializeComponent();

            wndGraphics = CreateGraphics(); //建立視窗畫布
            backBmp = new Bitmap(VIEW_W, VIEW_H);   //建立點陣圖
            backGraphics = Graphics.FromImage(backBmp); //建立背景畫布
            
            wndGraphics_2 = CreateGraphics();             //建立畫布
            backBmp_2 = new Bitmap(FORM_W, FORM_H);        //NEW一個新的點陣圖
            backGraphics_2 = Graphics.FromImage(backBmp_2);  //用點陣圖畫一個背景
            copy = new Bitmap(FORM_W, FORM_H);

            recorder_X = 0;
            recorder_Y = FORM_H / 2;
            //==================視窗初始=======================================================
            
            draw_axis(0);
            //===================================================================================                     
        }

        private Graphics wndGraphics;//視窗畫布
        private Graphics backGraphics;//背景頁畫布
        private Bitmap backBmp;//點陣圖

        public Bitmap curve;
        private const int VIEW_W = 240,
                          VIEW_H = 218,
                          matrixLength = 3;

        public float pointLocationX = 120,
                   pointLocationY = 218;
        public int traverseIndex = 1,
                   verticalIndex = 1;   //at first, put bitmap in the center of matrix
        public int verticalIndex_Z = 1,
                   traverseIndex_Z = 0;

        public bool firstFrame = true;
        public bool stop = false;

        public int tenSeconds = 10;

        Bitmap[,] finalBitmap = new Bitmap[matrixLength, matrixLength];
        public static Bitmap composeBmp = new Bitmap(matrixLength * VIEW_W, matrixLength * VIEW_H);
        public static Bitmap composeBmp_Z = new Bitmap(matrixLength * FORM_W, matrixLength * FORM_H);

        private void bGo_Click(object sender, EventArgs e)
        {
            bGo.Enabled = false;
            bGo.BackColor = Color.White;

            if (serverStarted)
            {
                updateUI("Server already started silly sausage!");
                return;
            }
            startScan();
        }

        private void startScan()
        {
            listBox1.DataSource = null;
            listBox1.Items.Clear();
            items.Clear();

            Thread bluetoothScanThread = new Thread(new ThreadStart(scan));
            bluetoothScanThread.Start();
        }

        BluetoothDeviceInfo[] devices;

        private void scan()
        {
            updateUI ("Starting scan..");
            BluetoothClient client = new BluetoothClient();
            devices = client.DiscoverDevicesInRange();
            updateUI("Scan complete");
            updateUI(devices.Length.ToString() + " devices discovered");

            foreach (BluetoothDeviceInfo d in devices)
            {
                items.Add(d.DeviceName);
            }

            updateDeviceList();
        }

        Guid mUUID = new Guid("00001101-0000-1000-8000-00805F9B34FB");
        bool serverStarted = false;

        private void updateUI(string message)
        {
            Func<string> del = delegate()
            {
                tbOutput.AppendText(message + System.Environment.NewLine);
                //tbOutput.AppendText(message);
                return "";
            };
            try
            {
                Invoke(del);
            }
            catch
            {
            }
        }

        private void updateDeviceList()
        {
            Func<int> del = delegate()
            {
                listBox1.DataSource = items;
                return 0;
            };
            Invoke(del);    //enable
        }

        BluetoothDeviceInfo deviceInfo;

        bool doubleClickedOrNot = false;

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            if (!doubleClickedOrNot)
            {
                try
                {
                    deviceInfo = devices.ElementAt(listBox1.SelectedIndex);     //bug
                }
                catch
                {
                }
                updateUI(deviceInfo.DeviceName + " was selected, attempting connect");

                if (pairDevice())
                {
                    updateUI("device paired");
                    updateUI("starting connect thread");
                    Thread bluetoothrClientThread = new Thread(new ThreadStart(ClientConnectThread));
                    bluetoothrClientThread.Start();
                }
                else
                {
                    updateUI("Pair failed");
                }

                doubleClickedOrNot = true;
            }
        }

        private void ClientConnectThread()
        {
            BluetoothClient client = new BluetoothClient();
            updateUI("attempting connect");
            client.BeginConnect(deviceInfo.DeviceAddress, mUUID, 
                this.BlueToothClientConnectCallBack, client);

            isConnected = true;
        }

        bool isConnected = false;

        Stream stream;

        string receive = string.Empty;
        string receiveGyro = string.Empty;
        string receiveVertical = string.Empty;
        string receiveYaw = string.Empty;
        string receiveFSR = string.Empty;
        string receiveAll = string.Empty;
        //int degree = 0;
        int intFinal = 0;
        string strGyro = "0";
        string strVertical = "0";
        string strYaw = "0";
        string strFSR = "0";
        string strFinal = "0";

        bool upToThirteen = false;

        int min = 48;   //0
        int max = 57;   //9 
        int dot = 46;   //.
        int minus = 45; //-
        int line = 124; //|
        int and = 38; //&
        int money = 36; //$
        int star = 42; //*

        void BlueToothClientConnectCallBack(IAsyncResult result)
        {
            BluetoothClient client = (BluetoothClient)result.AsyncState;
            try
            {
                client.EndConnect(result); //bug
            }
            catch
            {
                updateUI("failed!");
            }
            stream = client.GetStream(); //bug
            stream.ReadTimeout = 1000;  //decide how long we will try to read data
                       
            while (isConnected)
            {
                byte[] buffer = new byte[1];
                if (findTheDot(buffer, stream)) //find the "." in data which is receive from BT
                {
                    try
                    {   //=====================start to seperate the string of horizontal angle========================                     
                        int change = 0;
                        int meetDot_Gyro = 0;                        
                        while (change < 2)          //陀螺儀水平角度
                        {
                            stream.Read(buffer, 0, 1);
                            receive = Encoding.UTF8.GetString(buffer).ToString();
                            if (change == 0
                                && (min <= ASC(receive)
                                && ASC(receive) <= max  //whether data is between zero and nine or not
                                || ASC(receive) == dot  //"."
                                || ASC(receive) == minus    //"-"
                               ))    //"|" extra char to make we can catch data for seven times forever
                            {
                                if (meetDot_Gyro == 0 && (min <= ASC(receive) && ASC(receive) <= max) || ASC(receive) == minus || ASC(receive) == dot)
                                {
                                    if (ASC(receive) == dot)
                                        meetDot_Gyro = 1;
                                    else
                                    {
                                        receiveAll += receive;
                                        receiveGyro += receive;
                                    }
                                }
                                else if (meetDot_Gyro == 1 && (min <= ASC(receive) && ASC(receive) <= max))
                                    receiveAll += receive;
                            }
                            else if (ASC(receive) == and)
                            {
                                receiveAll += receive;
                                change = 2;
                            }
                        }
                        //=====================complete seperate the string of horizontal angle========================
                        //=====================start to seperate the string of vertical angle========================
                        int change_verti = 0;
                        int meetDot_verti = 0;
                        while (change_verti < 2)
                        {
                            stream.Read(buffer, 0, 1);
                            receive = Encoding.UTF8.GetString(buffer).ToString();
                            if (change_verti == 0
                                && (min <= ASC(receive)
                                && ASC(receive) <= max  //whether data is between zero and nine or not
                                || ASC(receive) == dot  //"."
                                || ASC(receive) == minus    //"-"                                    
                               ))    //"|" extra char to make we can catch data for seven times forever
                            {
                                if (meetDot_verti == 0 && (min <= ASC(receive) && ASC(receive) <= max) || ASC(receive) == minus || ASC(receive) == dot)
                                {
                                    if (ASC(receive) == dot)
                                        meetDot_verti = 1;
                                    else
                                    {
                                        receiveAll += receive;
                                        receiveVertical += receive;
                                    }
                                }
                                else if (meetDot_verti == 1 && (min <= ASC(receive) && ASC(receive) <= max))
                                    receiveAll += receive;
                            }
                            else if (ASC(receive) == money)
                            {
                                receiveAll += receive;
                                change_verti = 2;
                            }                            
                        }
                        //=====================complete seperate the string of vertical angle========================
                        //=====================start to seperate the string of angle of Yaw==========================
                        int change_Yaw = 0;
                        int meetDot_Yaw = 0;
                        bool meet_F = false;
                        while (change_Yaw == 0)
                        {
                            stream.Read(buffer, 0, 1);
                            receive = Encoding.UTF8.GetString(buffer).ToString();
                            if (change_Yaw == 0
                                && (min <= ASC(receive)
                                && ASC(receive) <= max  //whether data is between zero and nine or not
                                || ASC(receive) == dot  //"."
                                || ASC(receive) == minus    //"-"                                    
                               ))    //"|" extra char to make we can catch data for seven times forever
                            {
                                if (meetDot_Yaw == 0 && (min <= ASC(receive) && ASC(receive) <= max) || ASC(receive) == minus || ASC(receive) == dot)
                                {
                                    if (ASC(receive) == dot)
                                        meetDot_Yaw = 1;
                                    else
                                    {
                                        receiveAll += receive;
                                        receiveYaw += receive;
                                    }
                                }
                                else if (meetDot_Yaw == 1 && (min <= ASC(receive) && ASC(receive) <= max))
                                    receiveAll += receive;
                            }
                            else if (ASC(receive) == star)
                            {
                                receiveAll += receive;
                                change_Yaw = 2;
                            }
                            else if (ASC(receive) == 70)
                            {
                                meet_F = true;
                                break;
                            }
                        }
                        //=====================complete seperating the string of angle of Yaw========================
                        //=====================start to seperate the string of FSR Value=============================
                        int change_FSR = 0;
                        while (change_FSR == 0)
                        {
                            stream.Read(buffer, 0, 1);
                            receive = Encoding.UTF8.GetString(buffer).ToString();
                            if (change_FSR == 0 
                                && (min <= ASC(receive)
                                && ASC(receive) <= max  //whether data is between zero and nine or not
                                || ASC(receive) == dot  //"."
                                || ASC(receive) == minus    //"-"
                               ))    //"|" extra char to make we can catch data for seven times forever
                            {
                                receiveAll += receive;
                                receiveFSR += receive;
                            }
                            else if (ASC(receive) == line)
                            {
                                receiveAll += receive;
                                change_FSR = 1;
                            }
                        }
                        //=====================complete seperate the string of FSR Value==============================
                        if (meet_F == false)
                        {
                            strYaw = receiveYaw;
                            strGyro = receiveGyro;
                            strVertical = receiveVertical;
                            strFSR = receiveFSR;
                            //strFinal = strYaw + "Horizontal：" + strGyro + "　" + "Vertical：" + strVertical + "　" + "FSR：" + strFSR;
                            strFinal = "Yaw： " + finalYaw + "　" + "Pitch：" + strVertical + "　" + "Roll：" + strGyro + "　" + "FSR：" + strFSR;

                            updateUI(strFinal); //show data in the TB;

                            receive = string.Empty;
                            receiveGyro = string.Empty;
                            receiveVertical = string.Empty;
                            receiveYaw = string.Empty;
                            receiveFSR = string.Empty;
                            receiveAll = string.Empty; //clear sring     
                        }
                        else
                        {
                            receive = string.Empty;
                            receiveGyro = string.Empty;
                            receiveVertical = string.Empty;
                            receiveYaw = string.Empty;
                            receiveFSR = string.Empty;
                            receiveAll = string.Empty; //clear sring
                        }

                    }
                    catch
                    {
                        updateUI("failed");
                    }                
                }
            }
        }

        string myPin = "1234";  //password for BT

        private bool pairDevice()
        {
            if (!deviceInfo.Authenticated)  //if something go wrong, I guess
            {
                if (!BluetoothSecurity.PairRequest(deviceInfo.DeviceAddress, myPin))    //if password wasn't match
                {
                    return false;
                }
            }
            return true;
        }

        bool F = false;

        public static int ASC(string s) //get ASCII code
        {
            int N = Convert.ToInt32(s[0]);
            return N;
        }

        public bool findTheDot(byte[] buffer, Stream stream)    //find the first dit in the stream that I can pick up data at right position
        {
            string MReceive = "";
            bool findOrNot = false;

            while (true)    //pick up data until dot is found   
            {
                stream.Read(buffer, 0, 1);  //pick up data for one byte                
               
                MReceive = Encoding.UTF8.GetString(buffer).ToString();  //convert data into string type
                
                findOrNot = true;
                break;  //break out and return true
            }

            return findOrNot;
        }

        Bitmap img_org;

        private void Form1_Load(object sender, EventArgs e) //do these when load the form
        {
            pictureBox.Load("triangle.jpg");
            pbFsrCurve.Load("chou-1.jpg");
            img_org = (Bitmap)pictureBox.Image; //load image

            timer1.Interval = 1;    //make timer tick every ms, but it's not exactly accurate
            timer1.Start();
            timer3.Interval = 1;
            timer3.Start();

            curve = (Bitmap)pbFsrCurve.Image;

            timer5.Start();
        }

        public Size CalculateNewSize(int width, int height, double RotateAngle)
        {
            double r = Math.Sqrt(Math.Pow((double)width / 2d, 2d) + Math.Pow((double)height / 2d, 2d)); //半徑L
            double OriginalAngle = Math.Acos((width / 2d) / r) / Math.PI * 180d;  //對角線和X軸的角度θ
            double minW = 0d, maxW = 0d, minH = 0d, maxH = 0d; //最大和最小的 X、Y座標
            double[] drawPoint = new double[4];

            drawPoint[0] = (-OriginalAngle + RotateAngle) * Math.PI / 180d;
            drawPoint[1] = (OriginalAngle + RotateAngle) * Math.PI / 180d;
            drawPoint[2] = (180f - OriginalAngle + RotateAngle) * Math.PI / 180d;
            drawPoint[3] = (180f + OriginalAngle + RotateAngle) * Math.PI / 180d;

            foreach (double point in drawPoint) //由四個角的點算出X、Y的最大值及最小值
            {
                double x = r * Math.Cos(point);
                double y = r * Math.Sin(point);

                if (x < minW)
                    minW = x;
                if (x > maxW)
                    maxW = x;
                if (y < minH)
                    minH = y;
                if (y > maxH)
                    maxH = y;
            }

            return new Size((int)(maxW - minW), (int)(maxH - minH));
        }

        //旋轉圖片之函式
        //參數    image：要旋轉的圖片  RotateAngle：旋轉角度
        public Bitmap RotateBitmap(Bitmap image, float RotateAngle)
        {
            Size newSize = CalculateNewSize(image.Width, image.Height, RotateAngle);
            Bitmap rotatedBmp = new Bitmap(newSize.Width, newSize.Height);
            PointF centerPoint = new PointF((float)rotatedBmp.Width / 2f, (float)rotatedBmp.Height / 2f);
            Graphics g = Graphics.FromImage(rotatedBmp);

            g.TranslateTransform(centerPoint.X, centerPoint.Y);
            g.RotateTransform(RotateAngle);
            g.TranslateTransform(-centerPoint.X, -centerPoint.Y);

            g.DrawImage(image, (float)(newSize.Width - image.Width) / 2f, (float)(newSize.Height - image.Height) / 2f, image.Width, image.Height);
            g.Dispose();

            return rotatedBmp;
        }

        int Gyro_angle;
        private void timer1_Tick(object sender, EventArgs e)    //in tems of theorem, it'll do these every ms
        {
            // lbAngle.Text = strGyro;    //show degree next to the trackbar
            try
            {
                intFinal = System.Convert.ToInt32(strGyro);    //convert degree from string into int type,,,bug
            }
            catch
            {
            }
            Gyro_angle = (int)(intFinal + 180);  //assign trackber.value
            pictureBox.Image = RotateBitmap(img_org, (float)(Gyro_angle - 180)); //rotate the image
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 119)   //W
            {
                forward = true;
                left = !forward;
                right = !forward;
                backward = !forward;
            }
            else if (e.KeyChar == 97)   //A
            {
                left = true;
                forward = !left;
                right = !left;
                backward = !left;
            }
            else if (e.KeyChar == 115)  //S
            {
                backward = true;
                left = !backward;
                right = !backward;
                forward = !backward;
            }
            else if (e.KeyChar == 100)  //D
            {
                right = true;
                left = !right;
                forward = !right;
                backward = !right;
            }
            else if (e.KeyChar == 120)   //X
            {
                finalBitmap[traverseIndex, verticalIndex] = backBmp;
                stop = true;
                timer2.Stop();
                finalCompose(finalBitmap, 2, VIEW_H, VIEW_W, composeBmp);
                //finalCompose(verticalFinalBitmap, 3);
            }
            else if (e.KeyChar == 122)
            {
                verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z] = backBmp_2;
                stop = true;
                timer2.Stop();
                finalCompose(verticalFinalBitmap, 3, FORM_H, FORM_W, composeBmp_Z);
            }
            else if (e.KeyChar == 112)
            {
                timer2.Enabled = false;
            }
            else if (e.KeyChar == 103)
            {
                timer2.Enabled = true;
            }
        }

        public void finalCompose(Bitmap[,] bit, int which, int H, int W, Bitmap com)
        {
            for (int x = 0; x < matrixLength; x++)
            {
                for (int y = 0; y < matrixLength; y++)
                {
                    for (int w = 0; w < W; w++)
                    {
                        for (int h = 0; h < H; h++)
                        {
                            Color black = Color.FromArgb(0, 0, 0);
                            Color white = Color.FromArgb(255, 255, 255);

                            if (bit[x, y] == null && which == 2)
                            {
                                com.SetPixel((x * W) + w, (y * H) + h, black);
                            }
                            else if (bit[x,y] == null && which == 3)
                            {
                                if ((x == 1 && y == 0) ||
                                     (x == 2 && y == 0) ||
                                     (x == 1 && y == 2) ||
                                     (x == 2 && y == 2))
                                {
                                    com.SetPixel((x * W) + w, (y * H) + h, white);
                                }
                                else if ((x == 0 && y == 0) ||
                                         (x == 0 && y == 2))                                   
                                {
                                    if (w == 0)
                                        com.SetPixel((x * W) + w, (y * H) + h, black);
                                    else
                                        com.SetPixel((x * W) + w, (y * H) + h, white);
                                }
                                else if ((x == 2 && y == 1) ||
                                         (x == 1 && y == 1))
                                {
                                    if (h == FORM_H / 2)
                                    {
                                        com.SetPixel((x * W) + w, (y * H) + h, black);
                                    }
                                    else
                                    {
                                        com.SetPixel((x * W) + w, (y * H) + h, white);
                                    }
                                }
                            }
                            else
                            {
                                com.SetPixel((x * W) + w, (y * H) + h, bit[x, y].GetPixel(w, h));
                            }
                        }
                    }
                }
            }

            if (which == 2)
            {
                Form2 newForm2 = new Form2(composeBmp, ref stop, this);
                newForm2.Size = new System.Drawing.Size(matrixLength * VIEW_W + 16, matrixLength * VIEW_H + 38);
                newForm2.Show();
            }
            else if (which == 3)
            {
                Form3 newForm3 = new Form3(composeBmp_Z, ref stop, this);
                newForm3.Size = new System.Drawing.Size(matrixLength * FORM_W + 16, matrixLength * FORM_H + 38);
                newForm3.Show();
            }


        }

        //=========new=========
        int current_VerticalDegree = 0,
            next_VerticalDegree = 0,
            //未傾斜前的角度,
            pre_Degree;

        int drift,
            receiveYAW,
            finalYaw;

        bool getFirstDrift = false;

        bool VerticalDegreeChanged = false;

        int amountOfVerticalChanged,
            pathImageX = 15,
            pathImageY = 200;
        //=====================

        int heightGradient;
        private void timer2_Tick(object sender, EventArgs e)
        {
            current_VerticalDegree = System.Convert.ToInt16(strVertical);
            if (current_VerticalDegree != next_VerticalDegree)
            {
                VerticalDegreeChanged = true;
                //得到傾斜後的垂直角度變化量 = false;
            }
            else if (current_VerticalDegree == next_VerticalDegree)
                VerticalDegreeChanged = false;
            
            if (getFirstDrift == false)
            {
                drift = System.Convert.ToInt16(strYaw);
                getFirstDrift = true;
            }
            else
            {
                receiveYAW = System.Convert.ToInt16(strYaw);
                if (VerticalDegreeChanged == false)
                {
                    if (5 > current_VerticalDegree && current_VerticalDegree > -7)
                    {
                        finalYaw = receiveYAW - drift;
                        pre_Degree = finalYaw;
                    }
                    else
                    {
                        finalYaw = receiveYAW - amountOfVerticalChanged;
                        pre_Degree = finalYaw;
                    }
                }
                else if (VerticalDegreeChanged == true)
                {
                    amountOfVerticalChanged = receiveYAW - pre_Degree;
                    finalYaw = receiveYAW - amountOfVerticalChanged;
                }
            }

            next_VerticalDegree = current_VerticalDegree;

            if (finalYaw > 180)
                finalYaw -= 360;
            else if (finalYaw < -180)
                finalYaw += 360;

            //strFinal = "Yaw： " + finalYaw + "　" + "Pitch：" + strVertical + "　" + "Roll：" + strGyro + "　" + "FSR：" + strFSR;

            //updateUI(strFinal); //show data in the TB;
            
            //===================================
            if (firstFrame)
            {
                backGraphics.FillRectangle(Brushes.White, 0, 0, VIEW_W, VIEW_H);

                backGraphics.DrawLine(Pens.Black, 0, 0, 0, VIEW_H);
                backGraphics.DrawLine(Pens.Black, 0, 0, VIEW_W, 0);
                backGraphics.DrawLine(Pens.Black, VIEW_W, VIEW_H-1, 0, VIEW_H-1);
                backGraphics.DrawLine(Pens.Black, VIEW_W-1, VIEW_H, VIEW_W-1, 0);
            }

            firstFrame = false;

            int heightupperLimit = 5;
            int heightlowerLimit = -25;
                        
            heightGradient = 255 - (int)((height * scale - heightlowerLimit) * 255 / (heightupperLimit - heightlowerLimit));

            if (heightGradient > 255)
                heightGradient = 255;
            else if (heightGradient < 0)
                heightGradient = 0;

            Pen penGradient = new Pen(Color.FromArgb(heightGradient, 0, 0));

            //  main function
            if (pointLocationX <= VIEW_W
                && pointLocationY <= VIEW_H
                && pointLocationX >= 0
                && pointLocationY >= 0)
            {
                backGraphics.DrawEllipse(penGradient, pointLocationX, pointLocationY, 1, 1);

                wndGraphics.DrawImageUnscaled(backBmp, pathImageX, pathImageY);//把背景頁畫到視窗上
            }
            else if (pointLocationX < 0)
            {
                finalBitmap[traverseIndex, verticalIndex] = backBmp;
                if (traverseIndex - 1 >= 0)
                    traverseIndex -= 1;
                else
                {
                    timer2.Stop();
                    if (!stop)
                        MessageBox.Show("out of range!");
                }

                if (finalBitmap[traverseIndex, verticalIndex] != null)
                {
                    backBmp = finalBitmap[traverseIndex, verticalIndex];

                    backGraphics = Graphics.FromImage(finalBitmap[traverseIndex, verticalIndex]);
                    wndGraphics.DrawImageUnscaled(finalBitmap[traverseIndex, verticalIndex], pathImageX, pathImageY);//把背景頁畫到視窗上
                }
                else
                {
                    backBmp = new Bitmap(VIEW_W, VIEW_H);
                    backGraphics = Graphics.FromImage(backBmp);
                    firstFrame = true;
                    wndGraphics.DrawImageUnscaled(backBmp, pathImageX, pathImageY);//把背景頁畫到視窗上
                }

                pointLocationX = VIEW_W;
            }
            else if (pointLocationX > VIEW_W)
            {
                finalBitmap[traverseIndex, verticalIndex] = backBmp;
                if (traverseIndex + 1 <= 2)
                {
                    traverseIndex += 1;
                }
                else
                {
                    timer2.Stop();
                    if (!stop)
                        MessageBox.Show("out of range!");
                }

                if (finalBitmap[traverseIndex, verticalIndex] != null)
                {
                    backBmp = finalBitmap[traverseIndex, verticalIndex];

                    backGraphics = Graphics.FromImage(finalBitmap[traverseIndex, verticalIndex]);
                    wndGraphics.DrawImageUnscaled(finalBitmap[traverseIndex, verticalIndex], pathImageX, pathImageY);//把背景頁畫到視窗上
                }
                else
                {
                    backBmp = new Bitmap(VIEW_W, VIEW_H);
                    backGraphics = Graphics.FromImage(backBmp);
                    firstFrame = true;
                    wndGraphics.DrawImageUnscaled(backBmp, pathImageX, pathImageY);//把背景頁畫到視窗上
                }

                pointLocationX = 0;
            }
            else if (pointLocationY < 0)
            {
                finalBitmap[traverseIndex, verticalIndex] = backBmp;
                if (verticalIndex - 1 >= 0)
                    verticalIndex -= 1;
                else
                {
                    timer2.Stop();
                    if (!stop)
                        MessageBox.Show("out of range!");
                }

                if (finalBitmap[traverseIndex, verticalIndex] != null)
                {
                    backBmp = finalBitmap[traverseIndex, verticalIndex];

                    backGraphics = Graphics.FromImage(finalBitmap[traverseIndex, verticalIndex]);
                    wndGraphics.DrawImageUnscaled(finalBitmap[traverseIndex, verticalIndex], pathImageX, pathImageY);//把背景頁畫到視窗上
                }
                else
                {
                    backBmp = new Bitmap(VIEW_W, VIEW_H);
                    backGraphics = Graphics.FromImage(backBmp);
                    firstFrame = true;
                    wndGraphics.DrawImageUnscaled(backBmp, pathImageX, pathImageY);//把背景頁畫到視窗上
                }

                pointLocationY = VIEW_H;
            }
            else if (pointLocationY > VIEW_H)
            {
                finalBitmap[traverseIndex, verticalIndex] = backBmp;
                if (verticalIndex + 1 <= 2)
                    verticalIndex += 1;
                else
                {
                    timer2.Stop();
                    if (!stop)
                        MessageBox.Show("out of range!");
                }

                if (finalBitmap[traverseIndex, verticalIndex] != null)
                {
                    backBmp = finalBitmap[traverseIndex, verticalIndex];

                    backGraphics = Graphics.FromImage(finalBitmap[traverseIndex, verticalIndex]);
                    wndGraphics.DrawImageUnscaled(finalBitmap[traverseIndex, verticalIndex], pathImageX, pathImageY);//把背景頁畫到視窗上
                }
                else
                {
                    backBmp = new Bitmap(VIEW_W, VIEW_H);
                    backGraphics = Graphics.FromImage(backBmp);
                    firstFrame = true;
                    wndGraphics.DrawImageUnscaled(backBmp, pathImageX, pathImageY);//把背景頁畫到視窗上
                }

                pointLocationY = 0;
            }

            if (180 >= finalYaw && finalYaw >= -180)
            {
                pointLocationX += speedinMap * (float)Math.Cos(angle * Math.PI / 180) * (float)Math.Sin(finalYaw * Math.PI / 180);
                pointLocationY -= speedinMap * (float)Math.Cos(angle * Math.PI / 180) * (float)Math.Cos(finalYaw * Math.PI / 180);
            }

            if (traverseIndex == 0
                  && verticalIndex == 0
                  && !stop)
                this.Text = "upper left corner";
            else if (traverseIndex == 0
                  && verticalIndex == 1
                  && !stop)
                this.Text = "left side";
            else if (traverseIndex == 0
                  && verticalIndex == 2
                  && !stop)
                this.Text = "bottom left corner";
            else if (traverseIndex == 1
                  && verticalIndex == 0
                  && !stop)
                this.Text = "upper side";
            else if (traverseIndex == 1
                  && verticalIndex == 1
                  && !stop)
                this.Text = "middle";
            else if (traverseIndex == 1
                  && verticalIndex == 2
                  && !stop)
                this.Text = "bottom side";
            else if (traverseIndex == 2
                  && verticalIndex == 0
                  && !stop)
                this.Text = "upper right corner";
            else if (traverseIndex == 2
                  && verticalIndex == 1
                  && !stop)
                this.Text = "right side";
            else if (traverseIndex == 2
                  && verticalIndex == 2
                  && !stop)
                this.Text = "bottom right corner";
        //====================================================================================
            
            draw();            
        }

        public bool forward = true,
                    left = false,
                    right = false,
                    backward = false;
        
        int scaleX = 0;
        bool initialFSR;        
        
        private void timer3_Tick(object sender, EventArgs e)
        {
            int xMin = 59,
                xMax = 214,
                yMin = 75,
                yMax = 177,
                findOutY = 5;//

            int fsrMin = 265497;
            
            long fsrMax = 2400000;

            Color pixelColor;

            int red,
                green,
                blue;

            long fsr;
            
            //Color test;
            if (!initialFSR)
            {
                initialFSR = true;
                fsr = 2400000;
                scaleX = (int)((xMax - xMin) * (fsr - fsrMin) / (fsrMax - fsrMin));                
            }
            else
            {
                fsr = System.Convert.ToInt64(strFSR);
                if (fsr > 2400000)
                    fsr = 2400000;
                scaleX = (int)((xMax - xMin) * (fsr - fsrMin) / (fsrMax - fsrMin));
                if (scaleX <= 0)
                    scaleX = 0;
            }

            pbFsrCurve.Load("chou-1.jpg");
            curve = (Bitmap)pbFsrCurve.Image; //curve是點陣圖，將FSR圖片轉成點陣圖

            if (fsr >= 2400000)
                tb_pipelineSize.Text = "20.8";
            else if (fsr < 2400000 && fsr > 1500000)
                tb_pipelineSize.Text = "20.7";
            else if (fsr <= 1500000 && fsr > 833333)
                tb_pipelineSize.Text = "20.6";
            else if (fsr <= 833333 && fsr > 750000)
                tb_pipelineSize.Text = "20.5";
            else if (fsr <= 750000 && fsr > 666667)
                tb_pipelineSize.Text = "20.4";
            else if (fsr <= 666667 && fsr > 600000)
                tb_pipelineSize.Text = "20.3";
            else if (fsr <= 600000 && fsr > 583333)
                tb_pipelineSize.Text = "20.2";
            else if (fsr <= 583333 && fsr > 562500)
                tb_pipelineSize.Text = "20.1";
            else if (fsr <= 562500 && fsr > 555555)
                tb_pipelineSize.Text = "20.0";
            else if (fsr <= 555555 && fsr > 500000)
                tb_pipelineSize.Text = "19.9";
            else if (fsr <= 500000 && fsr > 437500)
                tb_pipelineSize.Text = "19.8";
            else if (fsr <= 437500 && fsr > 433334)
                tb_pipelineSize.Text = "19.7";
            else if (fsr <= 433334 && fsr > 350000)
                tb_pipelineSize.Text = "19.6";
            else if (fsr <= 350000 && fsr > 300000) //333333
                tb_pipelineSize.Text = "19.5";
            else if (fsr <= 300000 && fsr > 250000)
                tb_pipelineSize.Text = "19.6";

            for (int i = 52; i < yMax; i++)
            {
                pixelColor = curve.GetPixel(scaleX + xMin, i);

                red = pixelColor.R;
                green = pixelColor.G;
                blue = pixelColor.B;

                if (blue != 255 && blue >= 90 && green < 160 && red < 160)
                {
                    findOutY = i;
                    break;
                }
            }

            Graphics g;
            g = Graphics.FromImage(curve);

            Pen myPen = new Pen(Brushes.Red, 1);

            pbFsrCurve.Image = curve;

            g.DrawLine(myPen, scaleX + xMin, 188, scaleX + xMin, findOutY);
            g.DrawLine(myPen, 40, findOutY, scaleX + xMin, findOutY);
            g.Dispose();
        }

        /*----------- Map Scale ------------*/

        private float speedinMap = SPEED * 0.034f;
        
        int timer2_interval = 10;
        
        bool scale_YorN = false;
        
        int FPS = 60,
            scaleLocationX = 100,
            scaleLocationY = 447,
            scaleLocationY2 = 443;
        
        double scale;

        private void tb_PressKey(object sender, KeyPressEventArgs e)
        {
            
            if (e.KeyChar == 13)
            {
            //double scale = System.Convert.ToSingle(textBox1.Text) / RealMap;
                scale_YorN = true;
                scale = System.Convert.ToSingle(textBox1.Text);      //比例尺
                float Scale_Length = 200;   //比例尺長度
                speedinMap = speedinMap / (float)scale;     //speedinMap = 1.861f
                float howlong = 1 * (float)scale * 200;
                
                timer2.Interval = timer2_interval;
                wndGraphics.DrawLine(Pens.Black, scaleLocationX, scaleLocationY, scaleLocationX + Scale_Length, scaleLocationY);
                wndGraphics.DrawLine(Pens.Black, scaleLocationX, scaleLocationY2, scaleLocationX, scaleLocationY);//left
                wndGraphics.DrawLine(Pens.Black, scaleLocationX + Scale_Length, scaleLocationY2, scaleLocationX + Scale_Length, scaleLocationY);//right
                
                if (howlong >= 1 && howlong < 100)
                    label1.Text = System.Convert.ToString(howlong) + "cm" + "　( map scale 輸入1即為圖上的1pixel為1cm )";
                else if (100 <= howlong && scale < 100000)
                {
                    howlong /= 100;
                    label1.Text = System.Convert.ToString(howlong) + "m" + "　( map scale 輸入1即為圖上的1pixel為1cm )";
                }
                else if (scale >= 100000)
                {
                    howlong /= 100000;
                    label1.Text = System.Convert.ToString(howlong) + "km" + "　( map scale 輸入1即為圖上的1pixel為1cm )";
                }
                else if (howlong < 1)
                {
                    howlong *= 10;
                    label1.Text = System.Convert.ToString(howlong) + "mm" + "　( map scale 輸入1即為圖上的1pixel為1cm )";
                }

                timer4.Interval = 1000;
                 
            }
        }

        /*------------Walking Time Record-------------------*/
        public int walking_second= 0,
                   walking_min = 0,
                   walking_hour = 0;
        TimeSpan timespan;
        private void timer4_Tick(object sender, EventArgs e)
        {
            walking_second++;
            if (walking_second > 59)
            {
                walking_min++;
                if (walking_min > 59) 
                {
                    walking_hour++;
                    if (walking_hour > 11)
                    {
                        MessageBox.Show("Call it a day !");
                        walking_hour = 0;
                    }
                    walking_min = 0;  
                }
                walking_second = 0;   
            }
            timespan = new TimeSpan(walking_hour, walking_min, walking_second);
            Time_Label.Text = "探索時間：" + timespan.ToString();
        }

        private Graphics wndGraphics_2; //建立畫布視窗
        private Graphics wndGraphics_copy;

        private Graphics backGraphics_2;
        private Bitmap backBmp_2,
                       topBmp,
                       bottomBmp,
                       copy;

        private const int FORM_W = 300,
                          FORM_H = 250,
                          PADDING_X = 30,
                          PADDING_Y = 20,
                          RECOR0_DIAMETER = 1,

                          LOCATION_X = 265,
                          LOCATION_Y = 165;

        private const float SPEED = 5.7971f;

        private float x_axis = 240,
                      y_axis = 180,
                      recorder_X,
                      recorder_Y;

        private bool posi_nega;

        private Pen myPen = new Pen(Brushes.Black, 1);
        
        float ex_x,
              ex_y;
       
        int angle;

        private void draw()
        {           
            angle = System.Convert.ToInt16(strVertical);
            draw_record();
        }

        void draw_record()
        {
            Pen recordPen_link = new Pen(Brushes.Red, 1);
            //畫線
            process();
            
            backGraphics_2.DrawLine(recordPen_link, ex_x, ex_y, recorder_X, recorder_Y);
            /*
            backGraphics_2 = Graphics.FromImage(copy);
            
            copy = backBmp_2;
            */
            height += speedinMap * (float)Math.Sin(angle * Math.PI / 180);
            lbHeight.Text = "車體所在高度：" + System.Convert.ToString((float)(height * scale)) + "cm";
            /*
            if (posi_nega == true)
                topBmp = backBmp_2;
            else
                bottomBmp = backBmp_2;
            */
            wndGraphics_2.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);
        }

        Bitmap[,] verticalFinalBitmap = new Bitmap[matrixLength, matrixLength];

        float height = 0.0f;
        void process()
        {   
            ex_y = recorder_Y;
            ex_x = recorder_X;
            recorder_Y -= speedinMap * (float)Math.Sin(angle * Math.PI / 180);
            recorder_X += speedinMap * (float)Math.Cos(angle * Math.PI / 180);
            /*            
            if (topBmp == null)
            {
                topBmp = new Bitmap(FORM_W, FORM_H);
                backGraphics_2 = Graphics.FromImage(topBmp);
                posi_nega = true;
                draw_axis(posi_nega);

                backGraphics_2 = Graphics.FromImage(backBmp_2);
            }
            else if (topBmp != null)
            {*/
                if (posi_nega == true && recorder_X > x_axis + PADDING_X && recorder_Y > y_axis + PADDING_Y)
                {/*
                    backGraphics_2 = Graphics.FromImage(topBmp);
                    draw_axis(posi_nega);

                    posi_nega = false;
                    backGraphics_2 = Graphics.FromImage(bottomBmp);
                    draw_axis(posi_nega);
                    backGraphics_2 = Graphics.FromImage(backBmp_2);
                    draw_axis(posi_nega);

                    recorder_Y -= y_axis;
                    recorder_X = 30;
                    ex_x = recorder_X;
                    ex_y = recorder_Y;*/
                }
                else if (recorder_Y > FORM_H)
                {
                    verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z] = backBmp_2;

                    if (verticalIndex_Z < 2)
                        verticalIndex_Z++;
                    else
                    {
                        timer2.Stop();
                        MessageBox.Show("Out of Range");
                    }
                    /*
                    topBmp = backBmp_2;
                    backBmp_2 = bottomBmp;
                    backGraphics_2 = Graphics.FromImage(backBmp_2);
                    */

                    if (verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z] != null)
                    {
                        backBmp_2 = verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z];

                        backGraphics_2 = Graphics.FromImage(verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z]);
                        wndGraphics.DrawImageUnscaled(verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z], LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                    }
                    else
                    {
                        if ((traverseIndex_Z == 0 && verticalIndex_Z == 2))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_H);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(1);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                        else if ((traverseIndex_Z == 1 && verticalIndex_Z == 1) || (traverseIndex_Z == 2 && verticalIndex_Z == 1))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_H);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(2);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                        else if ((traverseIndex_Z == 1 && verticalIndex_Z == 2) || (traverseIndex_Z == 2 && verticalIndex_Z == 2))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_H);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(4);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                    }
                    

                    recorder_Y -= FORM_H;
                    ex_y = recorder_Y;
                    ex_x = recorder_X;

                }
                else if (recorder_X > FORM_W)
                {
                    verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z] = backBmp_2;
                    if (traverseIndex_Z < 2)
                        traverseIndex_Z++;
                    else
                    {
                        timer2.Stop();
                        MessageBox.Show("Out of Range");
                    }
                    //finalCompose(verticalFinalBitmap);
                    /*
                    posi_nega = false;
                    backGraphics_2 = Graphics.FromImage(bottomBmp);
                    draw_axis(posi_nega);

                    posi_nega = true;
                    backGraphics_2 = Graphics.FromImage(topBmp);
                    draw_axis(posi_nega);
                    backGraphics_2 = Graphics.FromImage(backBmp_2);
                    draw_axis(posi_nega);*/
                    if (verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z] != null)
                    {
                        backBmp_2 = verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z];

                        backGraphics_2 = Graphics.FromImage(verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z]);
                        wndGraphics.DrawImageUnscaled(verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z], LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                    }
                    else
                    {
                        if ((traverseIndex_Z == 1 && verticalIndex_Z == 0) || (traverseIndex_Z == 2 && verticalIndex_Z == 0) || (traverseIndex_Z == 1 && verticalIndex_Z == 2) ||(traverseIndex_Z == 2 && verticalIndex_Z == 2))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_W);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(4);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                        else if ((traverseIndex_Z == 1 && verticalIndex_Z == 1) || (traverseIndex_Z == 2 && verticalIndex_Z == 1))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_W);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(2);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                    }

                    recorder_X = 0;
                    ex_x = recorder_X;
                }
            //}
            /*
            if (bottomBmp == null)
            {
                bottomBmp = new Bitmap(FORM_W, FORM_H);
                backGraphics_2 = Graphics.FromImage(bottomBmp);
                posi_nega = false;
                draw_axis(posi_nega);

                backGraphics_2 = Graphics.FromImage(backBmp_2);
                posi_nega = true;
            }
            else if (bottomBmp != null)
            {*/
                else if (posi_nega == false && recorder_X > x_axis + PADDING_X && recorder_Y < PADDING_Y)
                {/*
                    backGraphics_2 = Graphics.FromImage(bottomBmp);
                    draw_axis(posi_nega);

                    posi_nega = true;
                    backGraphics_2 = Graphics.FromImage(topBmp);
                    draw_axis(posi_nega);
                    backGraphics_2 = Graphics.FromImage(backBmp_2);
                    draw_axis(posi_nega);

                    recorder_Y += y_axis;
                    recorder_X = 30;
                    ex_x = recorder_X;
                    ex_y = recorder_Y;*/
                }

                else if (recorder_Y < 0)
                {
                    verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z] = backBmp_2;

                    if (verticalIndex_Z > 0)
                        verticalIndex_Z--;
                    else
                    {
                        timer2.Stop();
                        MessageBox.Show("Out of Range");
                    }
                    /*
                    bottomBmp = backBmp_2;
                    backBmp_2 = topBmp;
                    backGraphics_2 = Graphics.FromImage(backBmp_2);

                    posi_nega = true;
                    */
                    if (verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z] != null)
                    {
                        backBmp_2 = verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z];

                        backGraphics_2 = Graphics.FromImage(verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z]);
                        wndGraphics.DrawImageUnscaled(verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z], LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                    }
                    else
                    {
                        if ((traverseIndex_Z == 0 && verticalIndex_Z == 0))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_H);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(1);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                        else if ((traverseIndex_Z == 1 && verticalIndex_Z == 1) || (traverseIndex_Z == 2 && verticalIndex_Z == 1))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_H);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(2);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                        else if ((traverseIndex_Z == 1 && verticalIndex_Z == 0) || (traverseIndex_Z == 2 && verticalIndex_Z == 0))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_H);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(4);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                    }


                    recorder_Y += FORM_H;
                    ex_y = recorder_Y;
                    ex_x = recorder_X;
                }
                else if (recorder_X < 0)
                {
                    verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z] = backBmp_2;

                    if (traverseIndex_Z > 0)
                        traverseIndex_Z--;
                    else
                    {
                        timer2.Stop();
                        MessageBox.Show("Out of Range");
                    }
                    /*
                    posi_nega = true;
                    backGraphics_2 = Graphics.FromImage(topBmp);
                    draw_axis(posi_nega);

                    posi_nega = false;
                    backGraphics_2 = Graphics.FromImage(bottomBmp);
                    draw_axis(posi_nega);
                    backGraphics_2 = Graphics.FromImage(backBmp_2);
                    draw_axis(posi_nega);*/
                    if (verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z] != null)
                    {
                        backBmp_2 = verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z];

                        backGraphics_2 = Graphics.FromImage(verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z]);
                        wndGraphics.DrawImageUnscaled(verticalFinalBitmap[traverseIndex_Z, verticalIndex_Z], LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                    }
                    else
                    {
                        if ((traverseIndex_Z == 0 && verticalIndex_Z == 0) || (traverseIndex_Z == 0 && verticalIndex_Z == 2))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_H);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(1);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                        else if ((traverseIndex_Z == 1 && verticalIndex_Z == 0) || (traverseIndex_Z == 1 && verticalIndex_Z == 2))
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_H);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(4);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }
                        else if (traverseIndex_Z == 1 && verticalIndex_Z == 1)
                        {
                            backBmp_2 = new Bitmap(FORM_W, FORM_H);
                            backGraphics_2 = Graphics.FromImage(backBmp_2);
                            draw_axis(2);
                            wndGraphics.DrawImageUnscaled(backBmp_2, LOCATION_X, LOCATION_Y);//把背景頁畫到視窗上
                        }

                    }


                    recorder_X = 0;
                    ex_x = recorder_X;
                }
           // }            
        }

        //================================================draw x-axis and y-axis===================================================
        void draw_axis(int which)
        {
            if (which == 0)
            {
                backGraphics_2.FillRectangle(Brushes.White, 0, 0, FORM_W, FORM_H);

                backGraphics_2.DrawLine(myPen, 0, 0, 0, FORM_H);                      //y-axis
                backGraphics_2.DrawLine(myPen, 0, FORM_H / 2, FORM_W, FORM_H / 2);     //x-axis               
            }
            else if (which == 1)
            {
                backGraphics_2.FillRectangle(Brushes.White, 0, 0, FORM_W, FORM_H);

                backGraphics_2.DrawLine(myPen, 0, 0, 0, FORM_H);                        //y-axis               
            }
            else if (which == 2)
            {
                backGraphics_2.FillRectangle(Brushes.White, 0, 0, FORM_W, FORM_H);

                backGraphics_2.DrawLine(myPen, 0, FORM_H / 2, FORM_W, FORM_H / 2);     //x-axis               
            }
            else
                backGraphics_2.FillRectangle(Brushes.White, 0, 0, FORM_W, FORM_H);

        }

        private void timer5_Tick(object sender, EventArgs e)
        {
            string str = "";
            tenSeconds -= 1;
            str = tenSeconds.ToString();
            if (tenSeconds > 0)
            {
                bGo.Text = "System will be ready for " + str + " second(s)";
            }
            else
            {
                bGo.Text = "Scan";
                bGo.Enabled = true;
                bGo.BackColor = Color.LightSkyBlue;
                timer5.Stop();
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes("R");   //convert the command into bytes
                stream.Write(buffer, 0, buffer.Length); //put it in buffer            
            }
            catch
            {
            }
        }

        public void sendFBHR(string command)
        {
            var buffer = System.Text.Encoding.UTF8.GetBytes(command);   //convert the command into bytes
            stream.Write(buffer, 0, buffer.Length); //put it in buffer            
        }

        bool getScale = false;
        bool which;     //看是半速還是全速
        float speedinMap_F;
        float speedinMap_H;

        private void btF_Click(object sender, EventArgs e)
        {
            which = true;
            sendFBHR("F");
            if (!scale_YorN)
            {
                scale = 0.15;      //比例尺
                float Scale_Length = 200;   //比例尺長度
                if (!getScale)
                {
                    speedinMap = speedinMap / (float)scale;     //speedinMap = 1.861f
                    speedinMap_F = speedinMap;
                    speedinMap_H = speedinMap_F / (255 / 120);
                    getScale = true;
                    
                }
                else if (getScale == true && which == true)
                    speedinMap = speedinMap_F;

                 
                float howlong = 1 * (float)scale * 200;

                timer2.Interval = timer2_interval;
                wndGraphics.DrawLine(Pens.Black, 177, 567, 177 + Scale_Length, 567);
                wndGraphics.DrawLine(Pens.Black, 177, 563, 177, 567);//left
                wndGraphics.DrawLine(Pens.Black, 177 + Scale_Length, 563, 177 + Scale_Length, 567);//right
                label1.Text = System.Convert.ToString(howlong) + "cm" + "　( map scale 輸入1即為圖上的1pixel為1cm )";
            }
            timer2.Start();
            timer4.Start();
        }

        private void btB_Click(object sender, EventArgs e)
        {
            sendFBHR("B");
        }

        private void btH_Click(object sender, EventArgs e)
        {
            which = false;
            sendFBHR("H");
            if (!scale_YorN)
            {
                scale = 0.15;      //比例尺
                float Scale_Length = 200;   //比例尺長度
                if (!getScale)
                {
                    speedinMap = (speedinMap / (float)scale) / (255 / 120);     //speedinMap = 1.861f
                    speedinMap_H = speedinMap;
                    speedinMap_F = speedinMap_H * (255 / 120);
                    getScale = true;
                }
                else if (getScale == true && which == false)
                    speedinMap = speedinMap_H;

                float howlong = 1 * (float)scale * 200;

                timer2.Interval = timer2_interval;
                wndGraphics.DrawLine(Pens.Black, 177, 567, 177 + Scale_Length, 567);
                wndGraphics.DrawLine(Pens.Black, 177, 563, 177, 567);//left
                wndGraphics.DrawLine(Pens.Black, 177 + Scale_Length, 563, 177 + Scale_Length, 567);//right
                label1.Text = System.Convert.ToString(howlong) + "cm" + "　( map scale 輸入1即為圖上的1pixel為1cm )";
            }
            timer2.Start();
            timer4.Start();
        }

        private void btR_Click(object sender, EventArgs e)
        {
            sendFBHR("R");
            timer2.Stop();
            timer4.Stop();
        }
    }
}
