using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Logicore.CommonControl;

public partial class FloatingEntry : ContentView
{
	int _placeholderFontSize = 14;
	int _titleFontSize = 12;
	int _topMargin = -22;

	public event EventHandler Completed;
	public static readonly BindableProperty TextProperty = BindableProperty.Create(nameof(Text), typeof(string), typeof(FloatingEntry), string.Empty, BindingMode.TwoWay, null, HandleBindingPropertyChangedDelegate);
	public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(FloatingEntry), string.Empty, BindingMode.TwoWay, null);
	public static readonly BindableProperty ErrorMessageProperty = BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(FloatingEntry), string.Empty, BindingMode.TwoWay, null);

	public static readonly BindableProperty HorizontalTextAlignmentProperty = BindableProperty.Create(nameof(HorizontalTextAlignment), typeof(TextAlignment), typeof(FloatingEntry), default(TextAlignment), BindingMode.TwoWay);
	public static readonly BindableProperty VerticalTextAlignmentProperty = BindableProperty.Create(nameof(VerticalTextAlignment), typeof(TextAlignment), typeof(FloatingEntry), TextAlignment.Center, BindingMode.TwoWay);
	public static readonly BindableProperty CharacterSpacingProperty = BindableProperty.Create(nameof(CharacterSpacing), typeof(double), typeof(FloatingEntry), default(double), BindingMode.TwoWay);

	public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(FloatingEntry), string.Empty, BindingMode.TwoWay, null);
	public static readonly BindableProperty ReturnTypeProperty = BindableProperty.Create(nameof(ReturnType), typeof(ReturnType), typeof(FloatingEntry), ReturnType.Default, BindingMode.TwoWay);
	public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(nameof(IsPassword), typeof(bool), typeof(FloatingEntry), false, BindingMode.TwoWay);
	public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(nameof(Keyboard), typeof(Keyboard), typeof(FloatingEntry), Keyboard.Default, BindingMode.TwoWay, coerceValue: (o, v) => (Keyboard)v ?? Keyboard.Default);
	public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(FloatingEntry), int.MaxValue, BindingMode.TwoWay);

	public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(nameof(BorderColor), typeof(Color), typeof(FloatingEntry), Colors.Transparent, BindingMode.TwoWay);
	public static readonly BindableProperty EntryBackColorProperty = BindableProperty.Create(nameof(EntryBackColor), typeof(Color), typeof(FloatingEntry), Colors.Transparent, BindingMode.TwoWay);
	public static readonly BindableProperty EntryTextColorProperty = BindableProperty.Create(nameof(EntryTextColor), typeof(Color), typeof(FloatingEntry), Colors.Transparent, BindingMode.TwoWay);

	public static readonly BindableProperty HasTitleProperty = BindableProperty.Create(nameof(HasTitle), typeof(bool), typeof(FloatingEntry), false, BindingMode.TwoWay);
	public static readonly BindableProperty HasLeftIconProperty = BindableProperty.Create(nameof(HasLeftIcon), typeof(bool), typeof(FloatingEntry), true, BindingMode.TwoWay);
	public static readonly BindableProperty HasRightIconProperty = BindableProperty.Create(nameof(HasRightIcon), typeof(bool), typeof(FloatingEntry), false, BindingMode.TwoWay, propertyChanged: (obj, oldValue, newValue) =>
	{
		System.Diagnostics.Debug.WriteLine($"HasRightIcon Property Changed : OldValue : {oldValue}, NewValue : {newValue}");
	});

	public static readonly BindableProperty RightIconSourceProperty = BindableProperty.Create(nameof(RightIconSource), typeof(ImageSource), typeof(FloatingEntry), default(ImageSource), BindingMode.TwoWay);

	public static readonly BindableProperty LeftIconSourceProperty = BindableProperty.Create(nameof(LeftIconSource), typeof(ImageSource), typeof(FloatingEntry), default(ImageSource), BindingMode.TwoWay);

	public static readonly BindableProperty LeftIconCommandProperty = BindableProperty.Create(nameof(LeftIconCommand), typeof(ICommand), typeof(FloatingEntry), null, BindingMode.TwoWay);

	//public static readonly BindableProperty DetailCommandProperty = BindableProperty.Create(nameof(DetailCommand), typeof(ICommand), typeof(CompareView), default(Command), BindingMode.TwoWay);
	public static readonly BindableProperty ReturnCommandProperty = BindableProperty.Create(nameof(ReturnCommand), typeof(ICommand), typeof(FloatingEntry), null, BindingMode.TwoWay);

	public static readonly BindableProperty RightIconCommandProperty = BindableProperty.Create(nameof(RightIconCommand), typeof(ICommand), typeof(FloatingEntry), null, BindingMode.TwoWay);

	public static readonly BindableProperty HasErrorProperty = BindableProperty.Create(nameof(HasError), typeof(bool), typeof(FloatingEntry), false, BindingMode.TwoWay);

	public static readonly BindableProperty IsLeftButtonVerticallyFlippedProperty = BindableProperty.Create(nameof(IsLeftButtonVerticallyFlipped), typeof(bool), typeof(FloatingEntry), false, BindingMode.TwoWay);

	public static readonly BindableProperty FontSizeProperty = BindableProperty.Create(nameof(FontSize), typeof(double), typeof(FloatingEntry), 16.0, BindingMode.TwoWay);

	public static readonly BindableProperty FontFamilyProperty = BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(FloatingEntry), "RobotoRegular", BindingMode.TwoWay);

	public static readonly BindableProperty TextChangedCommandProperty = BindableProperty.Create(nameof(TextChangedCommand), typeof(ICommand), typeof(FloatingEntry), null, BindingMode.TwoWay);

	public static readonly BindableProperty FocusCommandProperty = BindableProperty.Create(nameof(FocusCommand), typeof(ICommand), typeof(FloatingEntry), null, BindingMode.TwoWay);

	public static readonly BindableProperty UnFocusCommandProperty = BindableProperty.Create(nameof(UnFocusCommand), typeof(ICommand), typeof(FloatingEntry), null, BindingMode.TwoWay);

	public event EventHandler<TextChangedEventArgs> TextChangedField;
	public event EventHandler<FocusEventArgs> FocusedField;
	public event EventHandler<FocusEventArgs> UnFocusedField;

	static async void HandleBindingPropertyChangedDelegate(BindableObject bindable, object oldValue, object newValue)
	{
		var control = bindable as FloatingEntry;
		if (!control.EntryField.IsFocused)
		{
			if (!string.IsNullOrEmpty((string)newValue))
			{
				await control.TransitionToTitle(false);
			}
			else
			{
				await control.TransitionToPlaceholder(false);
			}
		}
	}

	#region Commands
	public ICommand ReturnCommand
	{
		get { return (ICommand)GetValue(ReturnCommandProperty); }
		set { SetValue(ReturnCommandProperty, value); }
	}

	public ICommand LeftIconCommand
	{
		get { return (ICommand)GetValue(LeftIconCommandProperty); }
		set { SetValue(LeftIconCommandProperty, value); }
	}
	public ICommand RightIconCommand
	{
		get { return (ICommand)GetValue(RightIconCommandProperty); }
		set { SetValue(RightIconCommandProperty, value); }
	}

	public ICommand TextChangedCommand
	{
		get { return (ICommand)GetValue(TextChangedCommandProperty); }
		set { SetValue(TextChangedCommandProperty, value); }
	}

	public ICommand FocusCommand
	{
		get { return (ICommand)GetValue(FocusCommandProperty); }
		set { SetValue(FocusCommandProperty, value); }
	}

	public ICommand UnFocusCommand
	{
		get { return (ICommand)GetValue(UnFocusCommandProperty); }
		set { SetValue(UnFocusCommandProperty, value); }
	}
	#endregion

	#region Properties
	public Color BorderColor
	{
		get { return (Color)GetValue(BorderColorProperty); }
		set { SetValue(BorderColorProperty, value); }
	}

	public Color EntryBackColor
	{
		get { return (Color)GetValue(EntryBackColorProperty); }
		set { SetValue(EntryBackColorProperty, value); }
	}


    public Color EntryTextColor
	{
		get { return (Color)GetValue(EntryTextColorProperty); }
		set { SetValue(EntryTextColorProperty, value); }
	}

	public int MaxLength
	{
		get { return (int)GetValue(MaxLengthProperty); }
		set { SetValue(MaxLengthProperty, value); }
	}

	public string Text
	{
		get => (string)GetValue(TextProperty);
		set => SetValue(TextProperty, value);
	}

	public string Placeholder
	{
		get => (string)GetValue(PlaceholderProperty);
		set => SetValue(PlaceholderProperty, value);
	}

	public TextAlignment HorizontalTextAlignment
	{
		get => (TextAlignment)GetValue(HorizontalTextAlignmentProperty);
		set => SetValue(HorizontalTextAlignmentProperty, value);
	}

	public TextAlignment VerticalTextAlignment
	{
		get => (TextAlignment)GetValue(VerticalTextAlignmentProperty);
		set => SetValue(VerticalTextAlignmentProperty, value);
	}

	public double CharacterSpacing
	{
		get => (double)GetValue(CharacterSpacingProperty);
		set => SetValue(CharacterSpacingProperty, value);
	}

	public string ErrorMessage
	{
		get => (string)GetValue(ErrorMessageProperty);
		set => SetValue(ErrorMessageProperty, value);
	}

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public ReturnType ReturnType
	{
		get => (ReturnType)GetValue(ReturnTypeProperty);
		set => SetValue(ReturnTypeProperty, value);
	}

	public bool HasLeftIcon
	{
		get { return (bool)GetValue(HasLeftIconProperty); }
		set { SetValue(HasLeftIconProperty, value); }
	}

	public bool HasTitle
	{
		get { return (bool)GetValue(HasTitleProperty); }
		set { SetValue(HasTitleProperty, value); }
	}

	public bool HasRightIcon
	{
		get { return (bool)GetValue(HasRightIconProperty); }
		set { SetValue(HasRightIconProperty, value); }
	}

	public bool IsPassword
	{
		get { return (bool)GetValue(IsPasswordProperty); }
		set { SetValue(IsPasswordProperty, value); }
	}

	public Keyboard Keyboard
	{
		get { return (Keyboard)GetValue(KeyboardProperty); }
		set { SetValue(KeyboardProperty, value); }
	}

	public ImageSource LeftIconSource
	{
		get { return (ImageSource)GetValue(LeftIconSourceProperty); }
		set { SetValue(LeftIconSourceProperty, value); }
	}

	public ImageSource RightIconSource
	{
		get { return (ImageSource)GetValue(RightIconSourceProperty); }
		set { SetValue(RightIconSourceProperty, value); }
	}
	public bool HasError
	{
		get { return (bool)GetValue(HasErrorProperty); }
		set { SetValue(HasErrorProperty, value); }
	}

	public bool IsLeftButtonVerticallyFlipped
	{
		get { return (bool)GetValue(IsLeftButtonVerticallyFlippedProperty); }
		set { SetValue(IsLeftButtonVerticallyFlippedProperty, value); }
	}

	public double FontSize
	{
		get { return (double)GetValue(FontSizeProperty); }
		set { SetValue(FontSizeProperty, value); }
	}

	public string FontFamily
	{
		get { return (string)GetValue(FontFamilyProperty); }
		set { SetValue(FontFamilyProperty, value); }
	}
	#endregion

	public FloatingEntry()
	{
		InitializeComponent();
		MainGrid.BindingContext = this;

		_placeholderFontSize = Convert.ToInt32(12);
		_titleFontSize = Convert.ToInt32(24);

		LabelTitle.TranslationX = 10;
		LabelTitle.FontSize = _placeholderFontSize;

		TransitionToPlaceholder(false);
	}

	public new void Focus()
	{
		if (IsEnabled)
		{
			EntryField.Focus();
		}
	}

	public void ForceUnFocusField()
	{
		EntryField.Unfocus();
	}

	async void Handle_Focused(object sender, FocusEventArgs e)
	{
		if (string.IsNullOrEmpty(Text))
		{
			await TransitionToTitle(true);
		}

		Color focusedColor =  (Color)Colors.Blue;
		EntryField.TextColor = focusedColor;
		if (BorderColor == Colors.Transparent)
		{
			BorderColor = focusedColor;
		}
		FocusedField?.Invoke(this, e);
		FocusCommand?.Execute(null);
	}

	async void Handle_Unfocused(object sender, FocusEventArgs e)
	{
		if (string.IsNullOrEmpty(Text))
		{
			await TransitionToPlaceholder(true);
		}

		Color unFocusedColor = Colors.White;
		EntryField.TextColor = unFocusedColor;
		BorderColor = unFocusedColor;
		UnFocusedField?.Invoke(this, e);
		UnFocusCommand?.Execute(null);
	}

	async Task TransitionToTitle(bool animated)
	{
		if (animated)
		{
			var t1 = LabelTitle.TranslateTo(0, _topMargin, 100);
			var t2 = SizeTo(_titleFontSize);
			await Task.WhenAll(t1, t2);
		}
		else
		{
			LabelTitle.TranslationX = 0;
			LabelTitle.TranslationY = _topMargin;
			LabelTitle.FontSize = _titleFontSize;
		}
	}

	async Task TransitionToPlaceholder(bool animated)
	{
		if (animated)
		{
			var t1 = LabelTitle.TranslateTo(0, -6, 100);
			var t2 = SizeTo(_placeholderFontSize);
			await Task.WhenAll(t1, t2);
		}
		else
		{
			LabelTitle.TranslationX = 0;
			LabelTitle.TranslationY = -6;
			LabelTitle.FontSize = _placeholderFontSize;
		}
	}

	void Handle_Tapped(object sender, EventArgs e)
	{
		if (IsEnabled)
		{
			EntryField.Focus();
		}
	}

	Task SizeTo(int fontSize)
	{
		var taskCompletionSource = new TaskCompletionSource<bool>();

		// setup information for animation
		Action<double> callback = input => { LabelTitle.FontSize = input; };
		double startingHeight = LabelTitle.FontSize;
		double endingHeight = fontSize;
		uint rate = 5;
		uint length = 100;
		Easing easing = Easing.Linear;

		// now start animation with all the setup information
		LabelTitle.Animate("invis", callback, startingHeight, endingHeight, rate, length, easing, (v, c) => taskCompletionSource.SetResult(c));

		return taskCompletionSource.Task;
	}

	void Handle_Completed(object sender, EventArgs e)
	{
		Completed?.Invoke(this, e);
	}

	protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		base.OnPropertyChanged(propertyName);

		if (propertyName == nameof(IsEnabled))
		{
			EntryField.IsEnabled = IsEnabled; 
		}
	}

	async void EntryField_TextChanged(System.Object sender, TextChangedEventArgs e)
	{
		if (e.NewTextValue != null)
		{
			TextChangedField?.Invoke(this, e);
			TextChangedCommand?.Execute(null);
		}

		if (string.IsNullOrEmpty(e.OldTextValue) && !string.IsNullOrEmpty(e.NewTextValue))
			await TransitionToTitle(true);
	}
}