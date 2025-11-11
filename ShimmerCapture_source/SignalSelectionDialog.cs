using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ShimmerCapture
{
    /// <summary>
    /// Dialog for selecting which signals to display on each graph
    /// Replaces the 90 static checkboxes with a modern, searchable interface
    /// </summary>
    public partial class SignalSelectionDialog : Form
    {
        private const int MAX_SIGNALS_PER_GRAPH = 30;

        // Available signals for each graph
        private List<string> availableSignals = new List<string>();

        // Current selections for each graph
        public List<string> Graph1Selections { get; private set; }
        public List<string> Graph2Selections { get; private set; }
        public List<string> Graph3Selections { get; private set; }

        // All signals (unfiltered) for reset purposes
        private List<string> allSignals = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="availableSignals">List of all available signal names</param>
        /// <param name="graph1Current">Currently selected signals for graph 1</param>
        /// <param name="graph2Current">Currently selected signals for graph 2</param>
        /// <param name="graph3Current">Currently selected signals for graph 3</param>
        public SignalSelectionDialog(
            List<string> availableSignals,
            List<string> graph1Current,
            List<string> graph2Current,
            List<string> graph3Current)
        {
            InitializeComponent();

            this.allSignals = availableSignals ?? new List<string>();
            this.availableSignals = new List<string>(this.allSignals);

            // Initialize selections
            this.Graph1Selections = graph1Current != null ? new List<string>(graph1Current) : new List<string>();
            this.Graph2Selections = graph2Current != null ? new List<string>(graph2Current) : new List<string>();
            this.Graph3Selections = graph3Current != null ? new List<string>(graph3Current) : new List<string>();

            // Setup UI
            InitializeControls();
            PopulateAllLists();
            UpdateSelectionCounts();
        }

        private void InitializeControls()
        {
            // Setup combo boxes with categories
            PopulateCategoryComboBoxes();

            // Wire up event handlers
            textBoxSearch1.TextChanged += (s, e) => FilterGraph1();
            textBoxSearch2.TextChanged += (s, e) => FilterGraph2();
            textBoxSearch3.TextChanged += (s, e) => FilterGraph3();

            comboBoxCategory1.SelectedIndexChanged += (s, e) => FilterGraph1();
            comboBoxCategory2.SelectedIndexChanged += (s, e) => FilterGraph2();
            comboBoxCategory3.SelectedIndexChanged += (s, e) => FilterGraph3();

            checkedListBox1.ItemCheck += (s, e) =>
            {
                // Use BeginInvoke to get updated check state
                this.BeginInvoke((MethodInvoker)(() => UpdateSelectionCount(1)));
            };

            checkedListBox2.ItemCheck += (s, e) =>
            {
                this.BeginInvoke((MethodInvoker)(() => UpdateSelectionCount(2)));
            };

            checkedListBox3.ItemCheck += (s, e) =>
            {
                this.BeginInvoke((MethodInvoker)(() => UpdateSelectionCount(3)));
            };

            // Select All / Clear All buttons
            buttonSelectAll1.Click += (s, e) => SelectAllInList(checkedListBox1);
            buttonSelectAll2.Click += (s, e) => SelectAllInList(checkedListBox2);
            buttonSelectAll3.Click += (s, e) => SelectAllInList(checkedListBox3);

            buttonClearAll1.Click += (s, e) => ClearAllInList(checkedListBox1);
            buttonClearAll2.Click += (s, e) => ClearAllInList(checkedListBox2);
            buttonClearAll3.Click += (s, e) => ClearAllInList(checkedListBox3);

            // Dialog buttons
            buttonOK.Click += ButtonOK_Click;
            buttonCancel.Click += ButtonCancel_Click;
            buttonApply.Click += ButtonApply_Click;
        }

        private void PopulateCategoryComboBoxes()
        {
            var categories = SignalGroupHelper.GetAvailableCategories(allSignals);

            foreach (var category in categories)
            {
                string displayName = SignalGroupHelper.GetCategoryDisplayName(category);
                comboBoxCategory1.Items.Add(new CategoryItem(category, displayName));
                comboBoxCategory2.Items.Add(new CategoryItem(category, displayName));
                comboBoxCategory3.Items.Add(new CategoryItem(category, displayName));
            }

            comboBoxCategory1.SelectedIndex = 0; // "All Signals"
            comboBoxCategory2.SelectedIndex = 0;
            comboBoxCategory3.SelectedIndex = 0;
        }

        private void PopulateAllLists()
        {
            PopulateList(checkedListBox1, Graph1Selections);
            PopulateList(checkedListBox2, Graph2Selections);
            PopulateList(checkedListBox3, Graph3Selections);
        }

        private void PopulateList(CheckedListBox listBox, List<string> currentSelections)
        {
            listBox.Items.Clear();

            foreach (string signal in availableSignals)
            {
                bool isChecked = currentSelections.Contains(signal);
                listBox.Items.Add(signal, isChecked);
            }
        }

        private void FilterGraph1()
        {
            ApplyFilters(checkedListBox1, textBoxSearch1.Text, GetSelectedCategory(comboBoxCategory1), Graph1Selections);
            UpdateSelectionCount(1);
        }

        private void FilterGraph2()
        {
            ApplyFilters(checkedListBox2, textBoxSearch2.Text, GetSelectedCategory(comboBoxCategory2), Graph2Selections);
            UpdateSelectionCount(2);
        }

        private void FilterGraph3()
        {
            ApplyFilters(checkedListBox3, textBoxSearch3.Text, GetSelectedCategory(comboBoxCategory3), Graph3Selections);
            UpdateSelectionCount(3);
        }

        private SignalGroupHelper.SignalCategory GetSelectedCategory(ComboBox comboBox)
        {
            if (comboBox.SelectedItem is CategoryItem item)
                return item.Category;
            return SignalGroupHelper.SignalCategory.All;
        }

        private void ApplyFilters(CheckedListBox listBox, string searchText, SignalGroupHelper.SignalCategory category, List<string> currentSelections)
        {
            // Get currently checked items before clearing
            var checkedItems = new HashSet<string>();
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                if (listBox.GetItemChecked(i))
                    checkedItems.Add(listBox.Items[i].ToString());
            }

            // Update current selections from checked items
            currentSelections.Clear();
            currentSelections.AddRange(checkedItems);

            // Apply filters
            var filtered = new List<string>(allSignals);

            if (category != SignalGroupHelper.SignalCategory.All)
                filtered = SignalGroupHelper.FilterByCategory(filtered, category);

            if (!string.IsNullOrWhiteSpace(searchText))
                filtered = SignalGroupHelper.FilterBySearchText(filtered, searchText);

            // Repopulate list
            listBox.Items.Clear();
            foreach (string signal in filtered)
            {
                bool isChecked = checkedItems.Contains(signal);
                listBox.Items.Add(signal, isChecked);
            }
        }

        private void SelectAllInList(CheckedListBox listBox)
        {
            for (int i = 0; i < listBox.Items.Count && i < MAX_SIGNALS_PER_GRAPH; i++)
            {
                listBox.SetItemChecked(i, true);
            }
            UpdateSelectionCountFromListBox(listBox);
        }

        private void ClearAllInList(CheckedListBox listBox)
        {
            for (int i = 0; i < listBox.Items.Count; i++)
            {
                listBox.SetItemChecked(i, false);
            }
            UpdateSelectionCountFromListBox(listBox);
        }

        private void UpdateSelectionCounts()
        {
            UpdateSelectionCount(1);
            UpdateSelectionCount(2);
            UpdateSelectionCount(3);
        }

        private void UpdateSelectionCount(int graphNumber)
        {
            CheckedListBox listBox = null;
            Label label = null;

            switch (graphNumber)
            {
                case 1:
                    listBox = checkedListBox1;
                    label = labelCount1;
                    break;
                case 2:
                    listBox = checkedListBox2;
                    label = labelCount2;
                    break;
                case 3:
                    listBox = checkedListBox3;
                    label = labelCount3;
                    break;
            }

            if (listBox == null || label == null)
                return;

            UpdateSelectionCountFromListBox(listBox, label);
        }

        private void UpdateSelectionCountFromListBox(CheckedListBox listBox, Label label = null)
        {
            int checkedCount = listBox.CheckedItems.Count;

            if (label != null)
            {
                label.Text = $"{checkedCount} of {MAX_SIGNALS_PER_GRAPH} selected";

                // Warning if at limit
                if (checkedCount >= MAX_SIGNALS_PER_GRAPH)
                {
                    label.ForeColor = Color.Red;
                    label.Text += " (LIMIT REACHED)";
                }
                else
                {
                    label.ForeColor = SystemColors.ControlText;
                }
            }

            // Enforce limit: prevent checking more than MAX
            if (checkedCount > MAX_SIGNALS_PER_GRAPH)
            {
                // Uncheck the most recently checked item
                for (int i = listBox.Items.Count - 1; i >= 0; i--)
                {
                    if (listBox.GetItemChecked(i))
                    {
                        listBox.SetItemChecked(i, false);
                        break;
                    }
                }
                MessageBox.Show($"Maximum {MAX_SIGNALS_PER_GRAPH} signals per graph allowed.",
                    "Selection Limit", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void CollectSelections()
        {
            Graph1Selections.Clear();
            Graph2Selections.Clear();
            Graph3Selections.Clear();

            foreach (var item in checkedListBox1.CheckedItems)
                Graph1Selections.Add(item.ToString());

            foreach (var item in checkedListBox2.CheckedItems)
                Graph2Selections.Add(item.ToString());

            foreach (var item in checkedListBox3.CheckedItems)
                Graph3Selections.Add(item.ToString());
        }

        private void ButtonOK_Click(object sender, EventArgs e)
        {
            CollectSelections();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ButtonApply_Click(object sender, EventArgs e)
        {
            CollectSelections();
            // Don't close the dialog, just collect selections
            MessageBox.Show("Signal selections applied. Click OK to close the dialog.",
                "Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Helper class for category combo box items
        private class CategoryItem
        {
            public SignalGroupHelper.SignalCategory Category { get; set; }
            public string DisplayName { get; set; }

            public CategoryItem(SignalGroupHelper.SignalCategory category, string displayName)
            {
                Category = category;
                DisplayName = displayName;
            }

            public override string ToString()
            {
                return DisplayName;
            }
        }
    }
}
