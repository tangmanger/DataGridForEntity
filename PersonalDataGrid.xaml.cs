using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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


namespace DataGridForEntity
{
    /// <summary>
    /// 内容：MyDataGird
    /// 作者：ztt
    /// 时间：2016年5月12日 09:51:16
    /// 备注：Make the wpf easy
    /// </summary>
    public partial class PersonalDataGrid : UserControl
    {
        public PersonalDataGrid()
        {
            InitializeComponent();
        }
        private bool _IsEnable = true;
        /// <summary>
        /// 设置datagrid内部控件是否可用
        /// </summary>
        public bool IsEnable
        {
            get
            {
                return _IsEnable;
            }
            set
            {
                _IsEnable = value;
            }
        }
        private bool _IsReadOnly = false;
        public bool IsReadOnly
        {
            get
            {
                return _IsReadOnly;
            }
            set
            {
                _IsReadOnly = value;
                MyDg.IsReadOnly = value;
            }
        }
        /// <summary>
        /// 获取或设置datagrid样式
        /// </summary>
        public Style MyDataGridStyle { get; set; }
        /// <summary>
        /// 获取或设置绑定的数据源
        /// </summary>
        public List<object> DataList
        {
            get
            {
                return (List<object>)MyDg.ItemsSource;
            }
            set
            {
                MyDg.ItemsSource = value;
                MainGrid.Children.Add(MyDg);
            }
        }
        /// <summary>
        /// 获取当前选择的第一行，为空则返回null
        /// </summary>
        public object SelectedItem { get { return MyDg.SelectedItem; } }
        /// <summary>
        /// commboxstyle样式
        /// </summary>
        public Style CommboxStyle { get; set; }
        /// <summary>
        /// 获取或设置数据源字典（当实体中存在其他list时使用）
        /// </summary>
        public Dictionary<string, List<object>> DicSoure { get; set; }
        /// <summary>
        /// 获取或设置实体字典（当实体中存在其他list时使用）
        /// </summary>
        public Dictionary<string, Type> EntityType { get; set; }
        /// <summary>
        /// 事件委托字典
        /// </summary>
        public Dictionary<string, RoutedEventHandler> DicDelegate { get; set; }

        private Type _T = null;
        /// <summary>
        /// 获取或设置此datagrid绑定的实体类型
        /// </summary>
        public Type T
        {
            get
            {
                return _T == null ? typeof(Type) : _T;
            }
            set
            {
                _T = value;
                MyDg = AddCloums(value);
            }
        }
        private DataGrid _MyDg = new DataGrid();
        public DataGrid MyDg
        {
            get
            {
                return _MyDg;
            }
            set
            {
                _MyDg = value;
                //   MainGrid.Children.Add(MyDg); 1 w   2
            }
        }
        public delegate void ClickDelegate();
        public ClickDelegate NewClick;
        private DataGrid AddCloums(Type T)
        {
            DataGrid dg = new DataGrid();
            DataGridBoundColumn dc = null;
            string AttributeCode = string.Empty;
            DataGridTemplateColumn DTC = new DataGridTemplateColumn();
            PropertyInfo[] Property = T.GetProperties();
            for (int index = 0; index < Property.Length; index++)
            {

                Console.WriteLine(Property[index].PropertyType);
                AttributeCode = "";
                //if (Property[index].PropertyType.Name == "String")
                if (((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes.Contains("DateTime"))//DateTime类型的特殊处理
                {
                    DTC = new DataGridTemplateColumn();
                    FrameworkElementFactory MyFactory = new FrameworkElementFactory(typeof(DatePicker));
                    Binding TimeBinding = new Binding(Property[index].Name);
                    TimeBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    TimeBinding.Mode = BindingMode.TwoWay;
                    MyFactory.SetValue(DatePicker.TextProperty, TimeBinding);
                    DataTemplate dt = new DataTemplate();
                    MyFactory.SetValue(DatePicker.IsEnabledProperty, IsEnable);
                    dt.VisualTree = MyFactory;
                    DTC.Header = ((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes.Replace("DateTime", string.Empty);
                    DTC.CellTemplate = dt;
                    DTC.CellEditingTemplate = dt;
                    dg.Columns.Add(DTC);
                }
                else if (((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes.Contains("Button"))//Button的特殊处理
                {
                    //ButtonSet
                    DTC = new DataGridTemplateColumn();
                    FrameworkElementFactory MyF = new FrameworkElementFactory(typeof(Button));
                    Binding ButtonBinding = new Binding(Property[index].Name);
                    MyF.SetValue(Button.ContentProperty, ButtonBinding);
                    DataTemplate dt = new DataTemplate();
                    MyF.SetValue(Button.IsEnabledProperty, IsEnable);
                    //事件绑定
                    MyF.AddHandler(Button.ClickEvent, new RoutedEventHandler(DicDelegate[((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes]));//根据字典 查找对面的事件
                    dt.VisualTree = MyF;
                    DTC.Header = ((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes.Replace("Button", string.Empty);
                    DTC.CellTemplate = dt;
                    DTC.CellEditingTemplate = dt;
                    dg.Columns.Add(DTC);
                }
                else if (((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes.Contains("List"))//特殊处理list的绑定，
                {
                    DTC = new DataGridTemplateColumn();
                    FrameworkElementFactory fef = new FrameworkElementFactory(typeof(ComboBox));
                    DataTemplate dt = new DataTemplate();
                    //Binding binding = new Binding();
                    //binding.Path = new PropertyPath("MarketIndicator");
                    /**根据指定键值获取指定列表
                     */
                    PropertyInfo[] CTProperty = EntityType[((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes].GetProperties();
                    for (int counts = 0; counts < CTProperty.Length; counts++)
                    {
                        Console.WriteLine(((NameAttribute)CTProperty[counts].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes);
                        if ((((NameAttribute)CTProperty[counts].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes) == "Display")
                        {
                            fef.SetValue(ComboBox.DisplayMemberPathProperty, CTProperty[counts].Name);
                        }
                        if (((NameAttribute)CTProperty[counts].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes == "Selected")
                        {
                            fef.SetValue(ComboBox.SelectedValuePathProperty, CTProperty[counts].Name);
                        }
                    }
                    Binding s = new Binding();
                    // s.Mode = BindingMode.Default;
                    s.Source = DicSoure[((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes];
                    fef.SetValue(ComboBox.ItemsSourceProperty, s);
                    Binding ValueBinding = new Binding(Property[index].Name);
                    ValueBinding.Mode = BindingMode.TwoWay;
                    ValueBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
                    fef.SetBinding(ComboBox.SelectedValueProperty, ValueBinding);
                    fef.SetValue(ComboBox.ForegroundProperty, Brushes.Black);
                    fef.SetValue(ComboBox.BackgroundProperty, Brushes.WhiteSmoke);
                    fef.SetValue(ComboBox.StyleProperty, CommboxStyle);
                    fef.SetValue(ComboBox.IsEnabledProperty, IsEnable);
                    fef.SetValue(ComboBox.IsReadOnlyProperty, IsReadOnly);
                    dt.VisualTree = fef;
                    DTC.Header = ((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes.Replace("List", string.Empty);
                    //     DTC.Binding = new Binding(Property[index].Name);
                    DTC.CellTemplate = dt;
                    DTC.CellEditingTemplate = dt;
                    dg.Columns.Add(DTC);
                }
                else
                {
                    dc = new DataGridTextColumn();
                    dc.Header = ((NameAttribute)Property[index].GetCustomAttributes(typeof(NameAttribute)).FirstOrDefault()).Attributes;
                    dc.Binding = new Binding(Property[index].Name);
                    dg.Columns.Add(dc);
                }
            }
            dg.AutoGenerateColumns = false;
            dg.Style = MyDataGridStyle;
            return dg;
        }
    }
}
