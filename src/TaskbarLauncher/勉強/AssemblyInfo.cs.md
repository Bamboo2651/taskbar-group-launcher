# AssemblyInfo.cs 解説

## このファイルの役割

**アセンブリ（ビルドされたexeやdll）全体に対する設定を記述するファイル。**  
自分でコードを書くファイルではなく、WPFプロジェクトを作ると**自動生成される**。  
基本的に触る必要はない。

---

## コード全文

```csharp
using System.Windows;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,
    ResourceDictionaryLocation.SourceAssembly
)]
```

---

## ブロックごとの解説

### ① `[assembly: ...]` とは

```csharp
[assembly: ThemeInfo(...)]
```

これは**アセンブリ属性（Assembly Attribute）**と呼ばれる書き方。  
`[assembly: ...]` と書くことで「このプロジェクト全体に対する設定」を指定できる。

通常の `[属性]` はクラスやメソッドに対して付けるが、  
`assembly:` をつけることでプロジェクト全体が対象になる。

```csharp
[SerializeField]          // クラスやフィールドへの属性（通常）
[assembly: ThemeInfo(...)] // プロジェクト全体への属性
```

---

### ② `ThemeInfo` とは

```csharp
[assembly: ThemeInfo(
    ResourceDictionaryLocation.None,          // テーマ固有のリソース辞書の場所
    ResourceDictionaryLocation.SourceAssembly // 汎用リソース辞書の場所
)]
```

WPFのテーマ（見た目のスタイル）リソースがどこにあるかをWPFランタイムに伝える設定。

2つの引数の意味：

| 引数 | 設定値 | 意味 |
|---|---|---|
| 第1引数 | `None` | テーマ固有のリソース辞書は使わない |
| 第2引数 | `SourceAssembly` | 汎用リソース辞書はこのアセンブリ自身の中にある |

StackBarはカスタムテーマを外部ファイルに分けていないので、  
第1引数は `None`（テーマファイルなし）になっている。

---

## まとめ

- 自動生成されるファイルで、基本的に編集しない
- WPFのテーマリソースの場所をシステムに伝えるだけの設定
- `[assembly: ...]` という書き方はプロジェクト全体への属性指定

---

## 補足：アセンブリとは

C#でコードをビルドすると `.exe` や `.dll` ファイルが生成される。  
この成果物のことを**アセンブリ**と呼ぶ。  
AssemblyInfo.cs はその「成果物全体への設定ファイル」という意味の名前。