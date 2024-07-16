using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using TabletopSpells.Helpers;
using TabletopSpells.Models;
using TabletopSpells.Models.Enums;
using TabletopSpells.Pages;

namespace TabletopSpells;
public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
        this.BindingContext = SharedViewModel.Instance; 
    }
}