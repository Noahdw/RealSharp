import QtQuick 2.9
import QtQuick.Layouts 1.3
import QtQuick.Controls 2.3
import QtQuick.Controls.Material 2.1

ApplicationWindow {
    id: window
    width: 600
    height: 520
    visible: true
    title: "RealSharp"

    MenuRibbon{
        id:menuRibbon
        width: 60
        height: parent.height
    }

    EditorTabView{
        anchors.left: menuRibbon.right
        id: editorTabView
        width: parent.width
        height: parent.height
    }

}
