PortPop
====

USBシリアルを使っていて、新しいポートに割り当てられるたびにデバイスマネージャを開くのが面倒だったので作ってみました。

シリアルポートが着脱されたときに、ポート名をバルーン表示でお知らせします。

おまけ機能で、接続されたドライブ名も表示します。(デフォルトはOFFです。設定メニューで有効にして下さい)

## Description

![USBシリアルを接続](img/screenshot1.png)

![設定メニュー](img/screenshot2.png)

## Requirement

* Microsoft Windows 7 以降
* Microsoft .net Framework 4.6 以降

## Install

[PortPopInstaller.msi](https://github.com/tomozh/PortPop/raw/master/PortPopInstaller/Release/PortPopInstaller.msi?raw=true) を実行してインストール、または、[PortPop.exe](https://github.com/tomozh/PortPop/blob/master/PortPop/bin/Release/PortPop.exe?raw=true) を、適当なフォルダに入れて実行して下さい。

実行後は、タスクトレイに常駐します。

インストーラでインストールした場合は、スタートアップへ自動的に登録されます。(ログオン時に自動起動します)

## Licence

[MIT](https://opensource.org/licenses/mit-license.php)

## Author

[tomozh](http://ore-kb.net)
