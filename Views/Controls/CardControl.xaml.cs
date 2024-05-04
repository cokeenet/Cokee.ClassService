using System.Windows;
using System.Windows.Controls;

using iNKORE.UI.WPF.Modern.Common.IconKeys;

namespace Cokee.ClassService.Views.Controls;

public partial class CardControl : UserControl
{
    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register("Icon", typeof(FontIconData), typeof(CardControl), new PropertyMetadata(new FontIconData()));

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(CardControl), new PropertyMetadata(""));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register("Description", typeof(string), typeof(CardControl), new PropertyMetadata(""));

    public static readonly DependencyProperty IsContentVisibleProperty =
        DependencyProperty.Register("IsContentVisible", typeof(bool), typeof(CardControl), new PropertyMetadata(false));

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register("Content", typeof(object), typeof(CardControl), new PropertyMetadata(null));

    public FontIconData Icon
    {
        get { return (FontIconData)GetValue(IconProperty); }
        set { SetValue(IconProperty, value); }
    }

    public string Title
    {
        get { return (string)GetValue(TitleProperty); }
        set { SetValue(TitleProperty, value); }
    }

    public string Description
    {
        get { return (string)GetValue(DescriptionProperty); }
        set { SetValue(DescriptionProperty, value); }
    }

    public bool IsContentVisible
    {
        get { return (bool)GetValue(IsContentVisibleProperty); }
        set { SetValue(IsContentVisibleProperty, value); }
    }

    public object Content
    {
        get { return GetValue(ContentProperty); }
        set { SetValue(ContentProperty, value); }
    }

    public CardControl()
    {
        InitializeComponent();
        DataContext = this;
    }
}