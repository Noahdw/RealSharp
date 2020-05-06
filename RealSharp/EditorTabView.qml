import QtQuick 2.9
import QtQuick.Layouts 1.3
import QtQuick.Controls 2.4

Rectangle{
    color: Qt.rgba(0.1,0.1,0.1,1);
    TabBar {
        id: tabBar
        width: parent.width
        height: 30
        TabButton {
            width: 100
            height: parent.height
            text: qsTr("main.cs")
        }
        TabButton {
            width: 100
            height: parent.height
            text: qsTr("test.cs")
        }
        TabButton {
            width: 100
            height: parent.height
            text: qsTr("pro.cs")
        }

    }

    StackLayout {
        id: stackLayout
        anchors.top: tabBar.bottom
        width: parent.width
        height: parent.height - tabBar.height
        currentIndex: tabBar.currentIndex
        onCurrentIndexChanged: {
            stackLayout.itemAt(currentIndex).forceActiveFocus();
        }
        Editor{
            width: parent.width
            height: parent.height
        }
        Editor{
            width: parent.width
            height: parent.height
        }
        Editor{
            width: parent.width
            height: parent.height
        }
    }
}
