using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Windows;
using Dapper;

namespace cc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string connectionString = "Data Source=dbs.db;Version=3;";
        private readonly IDataAccess dataAccess;
        private readonly IRandomNumberGenerator randomNumberGenerator;
        public MainWindow()
        {
            InitializeComponent();
            dataAccess = new DataAccess(connectionString);
            randomNumberGenerator = new RandomNumberGenerator();
            LoadData();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadData();
        }

        private void LoadData()
        {
            List<HarnessDrawing> harnessDrawingList = dataAccess.GetHarnessDrawings();
            List<Harness> harnessList = GenerateHarnesses(harnessDrawingList);
            dataGrid.ItemsSource = harnessList;
        }

        private List<Harness> GenerateHarnesses(List<HarnessDrawing> harnessDrawingList)
        {
            List<Harness> harnessList = new List<Harness>();
            Random random = randomNumberGenerator.GetRandomNumber();
            int count = 0;
            while (count < random.Next(3, 5))
            {
                HarnessDrawing harness_1 = GetRandomItem(harnessDrawingList, random);
                HarnessDrawing harness_2 = GetRandomItem(harnessDrawingList, random);

                List<HarnessWiring> harnessWiringList = dataAccess.GetHarnessWires(harness_1.Id);
                List<HarnessWiring> harnessWiringList2 = dataAccess.GetHarnessWires(harness_2.Id);

                bool hasDuplicate = harnessWiringList.Any(wiring1 => harnessWiringList2.Any(wiring2 => 
                    wiring1.Housing_1 == wiring2.Housing_1 || 
                    wiring1.Housing_2 == wiring2.Housing_2 ||
                    wiring1.Housing_1 == wiring2.Housing_2 
                ));

                Harness harness = new(
                    $"{harness_1.Harness}",
                    $"{harness_1.Harness_Version}",
                    $"{harness_1.Drawing}",
                    $"{harness_1.Drawing_Version}",
                    $"{harness_2.Harness}",
                    $"{harness_2.Harness_Version}",
                    $"{harness_2.Drawing}",
                    $"{harness_2.Drawing_Version}",
                    hasDuplicate
                );

                if (!harnessList.Any(existingHarness => existingHarness.Equals(harness)))
                {
                    harnessList.Add(harness);
                    count++;
                }
            }
            return harnessList;
        }

        private HarnessDrawing GetRandomItem(List<HarnessDrawing> harnessDrawingList, Random random)
        {
            int index = random.Next(harnessDrawingList.Count);
            return harnessDrawingList[index];
        }
    }

    public interface IDataAccess
    {
        List<HarnessDrawing> GetHarnessDrawings();
        List<HarnessWiring> GetHarnessWires(int harnessId);
    }

    public class DataAccess : IDataAccess
    {
        private readonly string _connectionString;

        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<HarnessDrawing> GetHarnessDrawings()
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<HarnessDrawing>("SELECT * FROM harness_drawing").AsList();
            }
        }

        public List<HarnessWiring> GetHarnessWires(int harnessId)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();
                return connection.Query<HarnessWiring>($"SELECT * FROM harness_wires WHERE harness_id = {harnessId}").AsList();
            }
        }
    }

    public interface IRandomNumberGenerator
    {
        Random GetRandomNumber();
    }

    public class RandomNumberGenerator : IRandomNumberGenerator
    {
        public Random GetRandomNumber()
        {
            return new Random();
        }
    }

    public class Harness
    {
        public Harness(
            string Harness_1,
            string Harness_1_version,
            string Drawing_1,
            string Drawing_1_version,
            string Harness_2,
            string Harness_2_version,
            string Drawing_2,
            string Drawing_2_version,
            bool Duplicate
            )
        {
            this.Harness_1 = Harness_1;
            this.Harness_1_version = Harness_1_version;
            this.Drawing_1 = Drawing_1;
            this.Drawing_1_version = Drawing_1_version;
            this.Harness_2 = Harness_2;
            this.Harness_2_version = Harness_2_version;
            this.Drawing_2 = Drawing_2;
            this.Drawing_2_version = Drawing_2_version;
            this.Duplicate = Duplicate;
        }

        public string Harness_1 { get; set; }
        public string Harness_1_version { get; set; }
        public string Drawing_1 { get; set; }
        public string Drawing_1_version { get; set; }
        public string Harness_2 { get; set; }
        public string Harness_2_version { get; set; }
        public string Drawing_2 { get; set; }
        public string Drawing_2_version { get; set; }
        public bool Duplicate { get; set; }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Harness otherHarness = (Harness)obj;
            return Harness_1 == otherHarness.Harness_1 &&
                   Harness_1_version == otherHarness.Harness_1_version &&
                   Drawing_1 == otherHarness.Drawing_1 &&
                   Drawing_1_version == otherHarness.Drawing_1_version &&
                   Harness_2 == otherHarness.Harness_2 &&
                   Harness_2_version == otherHarness.Harness_2_version &&
                   Drawing_2 == otherHarness.Drawing_2 &&
                   Drawing_2_version == otherHarness.Drawing_2_version &&
                   Duplicate == otherHarness.Duplicate;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (Harness_1?.GetHashCode() ?? 0);
                hash = hash * 23 + (Harness_1_version?.GetHashCode() ?? 0);
                hash = hash * 23 + (Drawing_1?.GetHashCode() ?? 0);
                hash = hash * 23 + (Drawing_1_version?.GetHashCode() ?? 0);
                hash = hash * 23 + (Harness_2?.GetHashCode() ?? 0);
                hash = hash * 23 + (Harness_2_version?.GetHashCode() ?? 0);
                hash = hash * 23 + (Drawing_2?.GetHashCode() ?? 0);
                hash = hash * 23 + (Drawing_2_version?.GetHashCode() ?? 0);
                hash = hash * 23 + Duplicate.GetHashCode();
                return hash;
            }
        }
    }

    public class HarnessDrawing
    {
        public int Id { get; set; }
        public string Harness_Version { get; set; }
        public string Harness { get; set; }
        public string Drawing_Version { get; set; }
        public string Drawing { get; set; }
    }

    public class HarnessWiring
    {
        public int Id { get; set; }
        public int Harness_Id { get; set; }
        public string Length { get; set; }
        public string Color { get; set; }
        public string Housing_1 { get; set; }
        public string Housing_2 { get; set; }
    }
}