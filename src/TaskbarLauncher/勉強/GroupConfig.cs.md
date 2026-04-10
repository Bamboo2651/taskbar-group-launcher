# GroupConfig.cs

## このファイルの役割

**グループ1つ分のデータを定義するクラス。**

StackBar では複数のアプリを「グループ」にまとめて管理する。  
その1グループ分の「ID」「名前」「アプリ一覧」を1つのまとまりとして扱うのがこのクラス。

JSONに保存されるデータの単位もこのクラスが基準になる。

---

## using（名前空間のインポート）

```csharp
using System;
using System.Collections.Generic;
```

| using | 用途 |
|---|---|
| `System` | `Guid.NewGuid()` でGUIDを生成するために必要 |
| `System.Collections.Generic` | `List<AppConfig>` を使うために必要 |

---

## クラス定義

```csharp
public class GroupConfig
```

`public` … プロジェクト内のどこからでもアクセスできる。  
`class GroupConfig` … グループ1件分のデータをまとめた設計図（クラス）。

このクラスのインスタンス（実体）1つが、StackBar の**グループ1つ**に対応する。

---

## プロパティ：Id

```csharp
public string Id { get; set; } = Guid.NewGuid().ToString();
```

グループを一意に識別するためのID。

**GUIDとは？**  
「Globally Unique Identifier」の略。`"a3f8c2d1-..."` のようなランダムな文字列で、世界中で重複しないように設計されている。

**なぜGUIDを使うのか？**  
グループ名は後から変更できるため、名前をIDに使うと別のグループと混同する恐れがある。  
変更されない固定のIDとしてGUIDを使うことで、どのグループかを確実に特定できる。

**実際の使われ方**  
タスクバーのショートカット（.lnk）にはこのIDが引数として含まれる。  
StackBar 起動時にIDを見て「どのグループのポップアップを開くか」を判断している。

---

## プロパティ：Name

```csharp
public string Name { get; set; } = "";
```

グループの表示名（例：`"開発ツール"`、`"ブラウザ"` など）。

`AppConfig.Name` はアプリ1つの名前であるのに対し、こちらは**グループ全体の名前**。  
設定アプリから自由に編集できる。初期値が `""` なのは `AppConfig` と同じ理由（null安全のため）。

---

## プロパティ：Apps

```csharp
public List<AppConfig> Apps { get; set; } = new List<AppConfig>();
```

このグループに登録されているアプリの一覧。  
`AppConfig` のリストとして持つことで、グループとアプリが親子関係になっている。

**`List<AppConfig>` とは？**  
`AppConfig` を何件でも順番に格納できる可変長のリスト。  
アプリを追加・削除するたびにこのリストが更新される。

**初期値を `new List<AppConfig>()` にする理由**  
`null` のままにしておくと、アプリを追加しようとしたときに `NullReferenceException` が発生する。  
最初から空のリストを用意しておくことで、グループ作成直後でも安全にアプリを追加できる。