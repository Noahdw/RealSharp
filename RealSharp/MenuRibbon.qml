import QtQuick 2.9
import QtQuick.Layouts 1.3
import QtQuick.Controls 2.4

Rectangle{
    color: Qt.rgba(0.1, 0.1, 0.1, 1)
    ColumnLayout{
        Item {
            width: parent.width
            height: 30
        }
        Rectangle{
            width: parent.width
            height: 50
            Text {
                text: qsTr("Edit")

            }
        }
        Rectangle{
            width: parent.width
            height: 50

            Text {
                text: qsTr("Debug")

            }

        }
        Rectangle{
            width: parent.width
            height: 50

            Text {
                text: qsTr("Projects")

            }

        }
    }
}
