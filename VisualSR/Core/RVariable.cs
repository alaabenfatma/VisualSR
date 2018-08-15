/*Copyright 2018 ALAA BEN FATMA

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.*/
using System.ComponentModel;
using System.Windows.Media;
using VisualSR.Properties;

namespace VisualSR.Core
{
    public enum RTypes
    {
        //Character refers to strings too, will be using the same properties.
        Character,

        //Numeric refers to Doubles,Integers,Decimals,Bytes and even complex values.
        Numeric,

        //Logical refers  to either TRUE or FALSE values. With that being said, you can call it a boolean type.
        Logical,

        /*While matrices are confined to two dimensions, arrays can be of any number of dimensions.
         *Factors are the r-objects which are created using a vector. It stores the vector along with the distinct values of the elements in the vector as labels.
         *A list is an R-object which can contain many different types of elements inside it like vectors, functions and even another list inside it.
         *A matrix is a two-dimensional rectangular data set. It can be created using a vector input to the matrix function.
         */
        ArrayOrFactorOrListOrMatrix,


        //Data frames are tabular data objects. Unlike a matrix in data frame each column can contain different modes of data.
        DataFrame,


        //This one can contain any value you put into it.
        Generic
    }

    public class RVariable : INotifyPropertyChanged
    {
        private ObjectPort _pp;
        private string _value;

        public RTypes Type;

        /// <summary>
        ///     Virtual type that works as a data container for R-data-like types.
        /// </summary>
        /// <param name="type"></param>
        public RVariable(RTypes type)
        {
            Type = type;
        }

        public ObjectPort ParentPort
        {
            get { return _pp; }
            set
            {
                _pp = value;
                switch (Type)
                {
                    case RTypes.ArrayOrFactorOrListOrMatrix:
                        ParentPort.StrokeBrush = Brushes.DodgerBlue;
                        break;
                    case RTypes.Character:
                        ParentPort.StrokeBrush = Brushes.Magenta;

                        break;
                    case RTypes.Logical:
                        ParentPort.StrokeBrush = Brushes.Red;

                        break;
                    case RTypes.Numeric:
                        ParentPort.StrokeBrush = Brushes.LawnGreen;
                        break;
                    case RTypes.DataFrame:
                        ParentPort.StrokeBrush = Brushes.Orange;
                        break;
                    default:
                        ParentPort.StrokeBrush = Brushes.LightGray;
                        break;
                }
            }
        }

        /// <summary>
        ///     Contains the <c>data</c> that will be <c>parsed</c>.
        /// </summary>
        public string Value
        {
            get { return _value; }
            set
            {
                _value = value;
                OnPropertyChanged("Value");
                ParentPort.OnDataChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}