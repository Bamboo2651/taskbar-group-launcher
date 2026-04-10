# MoveAppDialog.xaml

## このファイルの役割

**アプリの移動先グループを選択するダイアログのUI定義。**

設定アプリでアプリを右クリック →「別のグループに移動」を選んだときに表示される。  
グループの一覧をリストで表示し、移動先を選んで「移動」ボタンを押す操作フローになっている。

基本的な構造は `InputDialog.xaml` と同じだが、テキスト入力欄の代わりに**グループ選択リスト**を持つ点が異なる。

---

## Window の属性

```xml
<Window Title="StackBar - 移動先を選択"
        Height="300"
        Width="400"
        WindowStartupLocation="CenterOwner"
        ResizeMode="NoResize"
        Background="#1E1E2E">
```

`InputDialog.xaml` と異なる点のみ説明する。

| 属性 | 値 | InputDialogとの違い |
|---|---|---|
| `Title` | `"StackBar - 移動先を選択"` | 用途が分かるタイトルに変更 |
| `Height` | `300` | リストを表示するため100px高くなっている |

`WindowStartupLocation`・`ResizeMode`・`Background` は `InputDialog.xaml` と同じ理由のため省略。

---

## Grid レイアウト

```xml
<Grid.RowDefinitions>
    <RowDefinition Height="Auto"/>
    <RowDefinition Height="*"/>
    <RowDefinition Height="Auto"/>
</Grid.RowDefinitions>
```

| Row | 内容 | Height |
|---|---|---|
| 0 | タイトルテキスト | `Auto`（内容に合わせる） |
| 1 | グループ選択リスト | `*`（残りの高さをすべて使う） |
| 2 | ボタン | `Auto`（内容に合わせる） |

**`Height="*"` とは？**  
`Auto` や固定ピクセルで確保した残りのスペースをすべて使う設定。  
`InputDialog` の Row 1 は `Auto` だったが、こちらはリストを大きく表示したいため `*` にしている。  
ウィンドウの高さが変わっても ListBox がその分伸縮する。

---

## Row 0：TextBlock

```xml
<TextBlock Grid.Row="0"
           Text="移動先のグループを選択してください"
           Foreground="#FFFFFF"
           FontSize="14"
           Margin="0,0,0,12"/>
```

ダイアログの説明文。

**`x:Name` がない理由**  
`InputDialog` の `TitleText` は用途に応じてコードからテキストを変えるために `x:Name` が必要だった。  
このダイアログは「移動先選択」専用のため、テキストは常に固定。  
コードから操作する必要がないので `x:Name` は不要。

---

## Row 1：ListBox

```xml
<ListBox Grid.Row="1"
         x:Name="GroupListBox"
         Background="#2D2D2D"
         Foreground="#FFFFFF"
         BorderBrush="#444444"
         FontSize="14"/>
```

移動先のグループ一覧を表示するリスト。

**`ListBox` とは？**  
複数の項目を縦に並べて表示し、ユーザーが1つ選択できるUI要素。  
`TextBox`（文字入力）と違い、あらかじめ用意した選択肢から選ばせるときに使う。

**`x:Name="GroupListBox"` とは？**  
コード側から `GroupListBox.Items.Add(...)` でグループ名を追加したり、  
`GroupListBox.SelectedItem` で選択されたグループを取得するために命名している。  
XAMLでは項目の初期値を書かず、ダイアログを開くときにコードから動的に追加する。

---

## Row 2：StackPanel（ボタン）

```xml
<StackPanel Grid.Row="2"
            Orientation="Horizontal"
            HorizontalAlignment="Right"
            Margin="0,16,0,0">
    <Button Content="キャンセル" ... Click="Cancel_Click"/>
    <Button Content="移動" ... Click="Move_Click"/>
</StackPanel>
```

**「OK」→「移動」に変えた理由**  
ボタンのラベルは操作の結果が直感的に分かる言葉にするのがUXの基本。  
「OK」は汎用的すぎて何が起きるか分かりにくいため、「移動」という動詞にすることで操作の意味が明確になる。

**`Margin="0,16,0,0"` とは？**  
StackPanel の上に16pxの余白をつけている。  
`InputDialog` ではボタンのMarginで調整していたが、ここではStackPanel自体に上余白をつけてリストとの間にスペースを作っている。