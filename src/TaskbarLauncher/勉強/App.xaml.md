# App.xaml 解説

## このファイルの役割

WPFアプリの**アプリケーション全体の設定ファイル**。  
C#でいう「プロジェクト全体の設定」にあたる。  
アプリが起動したとき、最初に読み込まれる。

---

## コード全文

```xml
<Application x:Class="TaskbarLauncher.App"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="clr-namespace:TaskbarLauncher">
    <Application.Resources>
    </Application.Resources>
</Application>
```

---

## ブロックごとの解説

### ① `<Application ...>`

```xml
<Application x:Class="TaskbarLauncher.App"
```

- `Application` はWPFのアプリケーション本体を表すクラス
- `x:Class="TaskbarLauncher.App"` は「このXAMLに対応するC#クラスはどれか」を指定している
- 対応するC#ファイルは `App.xaml.cs` で、そこに `class App` が書いてある
- XAMLとC#は**セットで1つのクラス**を作る仕組みになっている（`partial class` という）

---

### ② `xmlns`（名前空間の宣言）

```xml
xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
xmlns:local="clr-namespace:TaskbarLauncher"
```

`xmlns` は XML NameSpace（名前空間）の略。  
「このXAMLファイルでどのクラスを使えるようにするか」を宣言している。

| 宣言 | 意味 |
|---|---|
| `xmlns="..."` | WPFの標準クラス（Button、Windowなど）を使えるようにする。プレフィックスなしで使える |
| `xmlns:x="..."` | XAMLの特殊機能（`x:Class`、`x:Name`など）を使えるようにする |
| `xmlns:local="..."` | 自分のプロジェクト（`TaskbarLauncher`名前空間）のクラスをXAMLから使えるようにする |

#### C#の `using` との対比

C#でいう `using` と同じ役割。

```csharp
// C#
using System.Windows;
using TaskbarLauncher;
```

```xml
<!-- XAMLでの同等の書き方 -->
xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
xmlns:local="clr-namespace:TaskbarLauncher"
```

---

### ③ `<Application.Resources>`

```xml
<Application.Resources>
</Application.Resources>
```

- アプリ全体で共有する**リソース（スタイル・色・テンプレートなど）**を定義する場所
- ここに書いたものはすべてのウィンドウ・コントロールから参照できる
- 今は空だが、たとえば共通のボタンスタイルや色をここに定義するとアプリ全体に適用できる

#### 使う場面の例

```xml
<Application.Resources>
    <!-- アプリ全体で使う色を定義 -->
    <Color x:Key="PrimaryColor">#5865F2</Color>

    <!-- アプリ全体で使うボタンスタイルを定義 -->
    <Style TargetType="Button">
        <Setter Property="Background" Value="#5865F2"/>
        <Setter Property="Foreground" Value="White"/>
    </Style>
</Application.Resources>
```

---

## まとめ

| 要素 | 役割 |
|---|---|
| `x:Class` | 対応するC#クラスの指定 |
| `xmlns` | 使うクラス群の宣言（C#のusingと同じ） |
| `Application.Resources` | アプリ全体で共有するスタイル・色などの定義場所 |

このファイル自体はシンプルだが、**`App.xaml.cs`（起動処理）とセットで動く**のがポイント。  
App.xamlは「器の設定」、App.xaml.csは「起動時の実際の処理」という役割分担になっている。