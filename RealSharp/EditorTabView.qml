import QtQuick 2.9
import QtQuick.Controls 1.4
import QtQuick.Layouts 1.3
import QtQuick.Controls 2.3

Rectangle{
    TabView{
        id: editorTabView
        height: parent.height
        anchors.fill: parent
        Tab{
            title: "temp.cs"
            Editor{
                width: parent.width
                height: parent.height
            }
        }
    }
}
