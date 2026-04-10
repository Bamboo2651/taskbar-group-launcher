# InputDialog.xaml

## このファイルの役割

**グループ名の入力など、ユーザーにテキストを入力させる汎用ダイアログのUI定義。**

「新しいグループを作成」などのボタンを押したときに表示される小さなウィンドウ。  
タイトル文・入力欄・OK/キャンセルボタンの3要素で構成されている。

`.xaml` はUIのレイアウトと見た目を定義するファイル。  
実際のボタン動作などの処理は対応する `InputDialog.xaml.cs` に書かれている。

---

## Window の属性

```xml
<Window x:Class="TaskbarLauncher.InputDialog"
        Title="StackBar"
        Height="200"
        Width="400"
        WindowStartupLocation="CenterOwner"
        ResizeMode="NoResize"
        Background="#1E1E2E">
```

| 属性 | 値 | 意味 |
|---|---|---|
| `x:Class` | `TaskbarLauncher.InputDialog` | このXAMLと対応するC#クラスを紐付ける |
| `Title` | `"StackBar"` | ウィンドウ上部に表示されるタイトルバーのテキスト |
| `Height` / `Width` | `200` / `400` | ウィンドウの初期サイズ（ピクセル） |
| `WindowStartupLocation` | `CenterOwner` | 親ウィンドウの中央に表示する |
| `ResizeMode` | `NoResize` | ウィンドウのサイズ変更を禁止する |
| `Background` | `#1E1E2E` | ウィンドウ全体の背景色（ダークテーマの紺色） |

**`CenterOwner` とは？**  
ダイアログを開いた親ウィンドウ（設定アプリ）の中央に自動配置する設定。  
`CenterScreen`（画面中央）と違い、親ウィンドウが移動していても常に親の中央に出る。

**`ResizeMode="NoResize"` とは？**  
入力ダイアログのサイズを変えても意味がないため、固定サイズにしている。  
最大化・最小化ボタンも非表示になる。

---

## Grid レイアウト

```xml
<Grid Margin="20">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
        <RowDefinition Height="Auto"/>
    </Grid.RowDefinitions>
```

`Grid` はWPFの基本レイアウトパネル。行・列を定義してその中に要素を配置する。

ここでは3行構成にしている。

| Row | 内容 |
|---|---|
| 0 | タイトルテキスト |
| 1 | テキスト入力欄 |
| 2 | OK / キャンセルボタン |

**`Height="Auto"` とは？**  
中に入れる要素の高さに合わせて自動でサイズが決まる設定。  
固定ピクセルを指定しないため、フォントサイズが変わっても崩れない。

**`Margin="20"` とは？**  
Grid全体にウィンドウの端から20pxの余白をつける。  
要素がウィンドウの端にぴったりくっつかないようにするための余白。

---

## Row 0：TitleText

```xml
<TextBlock Grid.Row="0"
           x:Name="TitleText"
           Foreground="#FFFFFF"
           FontSize="14"
           Margin="0,0,0,10"/>
```

ダイアログの説明文を表示するテキスト。

**`x:Name="TitleText"` とは？**  
このUI要素に名前をつけることで、C#コード側から `TitleText.Text = "グループ名を入力"` のように操作できる。  
XAMLで初期値を書かず、コードから状況に応じたテキストをセットすることで**汎用ダイアログとして使い回せる**。

**`Margin="0,0,0,10"` とは？**  
上・右・下・左の順で余白を指定する。  
ここでは下に10pxの余白をつけて、テキストと入力欄の間にスペースを作っている。

---

## Row 1：InputBox

```xml
<TextBox Grid.Row="1"
         x:Name="InputBox"
         Background="#2D2D2D"
         Foreground="#FFFFFF"
         BorderBrush="#444444"
         FontSize="14"
         Padding="8,6"
         Margin="0,0,0,16"/>
```

ユーザーがテキストを入力する欄。

| 属性 | 値 | 意味 |
|---|---|---|
| `x:Name` | `InputBox` | コード側から入力内容を `InputBox.Text` で取得するために命名 |
| `Background` | `#2D2D2D` | 入力欄の背景色（暗めのグレー） |
| `Foreground` | `#FFFFFF` | 入力文字の色（白） |
| `BorderBrush` | `#444444` | 入力欄の枠線の色 |
| `Padding` | `8,6` | 入力欄の内側の余白（横8px・縦6px）。文字が枠にくっつかないようにする |
| `Margin` | `0,0,0,16` | 下に16pxの余白。入力欄とボタンの間にスペースを作る |

---

## Row 2：StackPanel（ボタン）

```xml
<StackPanel Grid.Row="2"
            Orientation="Horizontal"
            HorizontalAlignment="Right">
    <Button Content="キャンセル"
            Width="90"
            Margin="0,0,8,0"
            Background="#2D2D2D"
            Foreground="#888888"
            BorderBrush="#444444"
            Cursor="Hand"
            Click="Cancel_Click"/>
    <Button Content="OK"
            Width="90"
            Background="#5865F2"
            Foreground="#FFFFFF"
            BorderThickness="0"
            Cursor="Hand"
            Click="OK_Click"/>
</StackPanel>
```

**`StackPanel` とは？**  
子要素を縦または横に並べるシンプルなレイアウトパネル。  
`Orientation="Horizontal"` で横並びにしている。

**`HorizontalAlignment="Right"` とは？**  
StackPanel全体をRow 2の右端に寄せる設定。  
「キャンセル → OK」の順でウィンドウ右側に並ぶ。

**ボタンの違い**

| | キャンセル | OK |
|---|---|---|
| 背景色 | `#2D2D2D`（グレー） | `#5865F2`（青紫） |
| 文字色 | `#888888`（薄いグレー） | `#FFFFFF`（白） |
| 役割 | 目立たせない（サブアクション） | 目立たせる（メインアクション） |

OKボタンを青紫にすることで視線が自然にOKへ向かうデザインになっている。

**`Cursor="Hand"` とは？**  
ボタンにマウスを乗せたときにカーソルを手の形に変える。  
クリックできることを視覚的に伝えるUXの工夫。

**`Click="Cancel_Click"` / `Click="OK_Click"` とは？**  
ボタンをクリックしたときに呼び出すC#のメソッド名を指定している。  
実際の処理（ダイアログを閉じる・入力値を返すなど）は `InputDialog.xaml.cs` に書かれている。