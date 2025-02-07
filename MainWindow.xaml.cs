using System.Data;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Notizbuch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {


        Notiz? _AktuelleNotiz;
        public Notiz AktuelleNotiz
        {
            get => _AktuelleNotiz;
            set
            {
                _AktuelleNotiz = value;
                if (value != null) tbxNotiz.Text = value.Inhalt;
                else tbxNotiz.Clear();
                tbxNotiz.IsEnabled = value != null;


                // alternativ:
                //tbxNotiz.Text = value?.Inhalt ?? "";

            }
        }

        void listeAktualisieren(string suchtext = "")
        {
            var gewählteKat = cbxKategorie.SelectedItem as Kategorie? ?? Kategorie.Alle;
            var query = Notiz.Notizen.Values.Where(notiz => notiz.Inhalt.Contains(suchtext) && (gewählteKat == Kategorie.Alle || notiz.Kategorie == gewählteKat)).OrderBy(notiz => notiz.Kategorie)
                .OrderBy(notiz => notiz.Kategorie)
                .ThenBy(notiz => notiz.Inhalt.Split('\r', '\n')[0]);



            //lbxNotizen.ItemsSource = query; // komplette Collection als Items der ListBox 
            //ohne lbxNotizen.Items.Clear() bleibt AktuelleNotiz unverändert und kann für erneute Auswahl verwendet werden.
            DataContext = query; // Ergebnismenge wird (hier) dem Window zur Verfügung gestellt und kann dort von allen Elementen nach Bedarf verwendet werden
            lbxNotizen.SelectedItem = AktuelleNotiz;

        }

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_LOADED;
        }
        public void MainWindow_LOADED(object sender, RoutedEventArgs e)
        {


            new Notiz(Kategorie.Geburtstage, "Mutter: 18.03.1945");
            new Notiz(Kategorie.Geburtstage, "Vater: 21.08.1940");
            new Notiz(Kategorie.Internet, "www.google.com\r\nHello World");
            new Notiz(Kategorie.Urlaub, "Mallorca\r\nwar nicht gut!");
            new Notiz(Kategorie.Wichtig, "Steuererklärung\r\nmuss noch gemacht werden!!!");
            new Notiz(Kategorie.Wichtig, "Steuererklärung2022\r\nmuss noch gemacht werden!!!");

            foreach (var kat in Enum.GetValues(typeof(Kategorie)))
            {
                cbxKategorie.Items.Add(kat);
            }

            listeAktualisieren();


        }

        private void cbxKategorie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listeAktualisieren();
            btnNeu.IsEnabled = (Kategorie)cbxKategorie.SelectedItem != Kategorie.Alle;
            
            Resources["rscFarbe3"] = btnNeu.IsEnabled ? Brushes.DarkGray : Brushes.Red;


        }

        private void lbxNotizen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _AktuelleNotiz = (Notiz)lbxNotizen.SelectedItem;

            if (lbxNotizen.SelectedIndex > -1)
            {
                btnLöschen.IsEnabled = true;

            }


        }

        private void tbxNotiz_TextChanged(object sender, TextChangedEventArgs e)
        {

            btnSpeichern.IsEnabled = AktuelleNotiz != null && tbxNotiz.Text != "";


        }

        private void btnSpeichern_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnSuche_Click(object sender, RoutedEventArgs e)
        {



            // Logik, die beim Klicken auf den Button ausgeführt wird.
            MessageBox.Show("Suchbutton geklickt!");

        }
    }

    class DatumKonverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            /// <summary>
            /// Konvertiert gebundene Daten (aus Bindungsquelle, z.B. Objekteigenschaft)
            /// in Richtung Element-Attribut, in dem die Daten verwendet werden sollen (Bindungsziel)
            /// </summary>
            /// <param name="value">die zu konvertierenden Daten aus der Bindungsquelle</param>
            /// <param name="targetType">der erforderliche Datentyp für das Bindungsziel</param>
            /// <param name="parameter">zusätzlicher Parameter (XAML: ConverterParameter)</param>
            /// <param name="culture">Lokalisierungsinformationen (XAML: ConverterCulture)</param>
            /// <returns>die konvertierten Daten für das Bindungsziel; müssen dem TargetType entsprechen </returns>

            object retValue = null;
            // value ist IMMER vom Typ object, muss deshalb zur Weiterverarbeitung konvertiert werden
            var notizDatum = (DateTime)value;
            // bei Parameter-Initialisierung in XAML kann es nur String sein
            string para = ((string)parameter);
            // targetType kann ausgewertet werden, um Konverter für unterschiedliche Attribute einsetzen zu können
            if (targetType == typeof(string)) // TextBlock.Text benötigt string
                                              // es soll ein Datum im kurzen deutschen Format (TT.MM.JJJJ) angezeigt werden
                retValue = notizDatum.ToShortDateString();
            else if (targetType == typeof(Brush)) // Vorder- oder Hintergrundfarbe benötigt Brush
                                                  // Parameter wird hier verwendet, um Konverter für unterschiedliche Attribute mit gleichem Datentyp einsetzen zu können
                retValue = para == "vordergrund" ? notizDatum < DateTime.Today ? Brushes.DarkBlue : Brushes.Red : para == "hintergrund" ? notizDatum < DateTime.Today ? Brushes.Orange : Brushes.LightGreen : null; // Dummy für weitere Parameter
            return retValue;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();

        }


    }

    /// <summary>
    /// mögliche Kategorien für Notizen
    /// </summary>
    /// 

    public enum Kategorie { Alle, Wichtig, Urlaub, Geburtstage, Internet, Sonstiges }

    public class Notiz
    {

        /// <summary>
        /// Auflistung aller Notizen
        /// </summary>
        public static Dictionary<Guid, Notiz> Notizen = new Dictionary<Guid, Notiz>();
        /// <summary>
        /// eindeutige ID einer Notiz
        /// </summary>
        public Guid ID { get; private set; }
        /// <summary>
        /// Kategorie einer Notiz
        /// </summary>
        public Kategorie Kategorie { get; private set; }
        /// <summary>
        /// Inhalt (Text) einer Notiz
        /// </summary>
        public string Inhalt { get; set; }
        /// <summary>
        /// Datum/Zeit der Erstellung einer Notiz
        /// </summary>
        public DateTime ErstelltAm { get; private set; }
        /// <summary>
        /// legt eine neue Notiz an und fügt sie der Auflistung hinzu
        /// </summary>
        /// <param name="kat">Kategorie der neuen Notiz</param>
        /// <param name="text">Inhalt der neuen Notiz</param>
        public Notiz(Kategorie kat, string text)
        {
            this.Kategorie = kat;
            this.Inhalt = text;
            this.ID = Guid.NewGuid();
            this.ErstelltAm = DateTime.Now;
            Notizen.Add(this.ID, this);
        }
        /// <summary>
        /// entfernt eine Notiz aus der Auflistung
        /// </summary>
        /// <param name="notiz">die zu entfernende Notiz</param>
        /// <returns>true, wenn die Notiz erfolgreich entfernt werden konnte, sonst false</returns>
        public static bool Entfernen(Notiz notiz) => Notizen.Remove(notiz.ID);
    }
}