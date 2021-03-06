﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleExtraction
{
    public class DataUpdate : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        int _imageCount;
        /// <summary>
        /// Search image count result
        /// </summary>
        public int imageCount
        {
            get { return _imageCount; }

            set
            {
                if (_imageCount != value)
                {
                    _imageCount = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("imageCount"));
                }
            }
        }

        int _imageWidth;
        /// <summary>
        /// Image width
        /// </summary>
        public int imageWidth
        {
            get { return _imageWidth; }

            set
            {
                if (_imageWidth != value)
                {
                    _imageWidth = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("imageWidth"));
                }
            }
        }

        int _imageHeight;
        /// <summary>
        /// Image height
        /// </summary>
        public int imageHeight
        {
            get { return _imageHeight; }

            set
            {
                if (_imageHeight != value)
                {
                    _imageHeight = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("imageHeight"));
                }
            }
        }

        int _finishedCount;
        /// <summary>
        /// Image of finished count
        /// </summary>
        public int finishedCount
        {
            get { return _finishedCount; }

            set
            {
                if (_finishedCount != value)
                {
                    _finishedCount = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("finishedCount"));
                }
            }
        }

        long _autoCheckTime;
        /// <summary>
        /// Image of finished count
        /// </summary>
        public long autoCheckTime
        {
            get { return _autoCheckTime; }

            set
            {
                if (_autoCheckTime != value)
                {
                    _autoCheckTime = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("autoCheckTime"));
                }
            }
        }

        int _faceCheckSuccessed;
        /// <summary>
        /// Image of finished count
        /// </summary>
        public int faceCheckSuccessed
        {
            get { return _faceCheckSuccessed; }

            set
            {
                if (_faceCheckSuccessed != value)
                {
                    _faceCheckSuccessed = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("faceCheckSuccessed"));
                }
            }
        }
    }
}
