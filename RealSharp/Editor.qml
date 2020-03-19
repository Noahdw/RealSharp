import QtQuick 2.9
import QtQuick.Layouts 1.3
import QtQuick.Controls 2.3
import QtQuick.Controls.Material 2.1
import Editor 1.0

Rectangle {
    focus: true
    property int lineHeight: 16
    property int textBegin: 35
    property int characterWidth: 8
    property int fontSize: 14
    property bool drawCursor: true
    Canvas {
        id: editorCanvas
        width: parent.width
        height: parent.height
        onPaint: {
            var ctx = getContext("2d");
            var y = editorModel.cursorY * lineHeight;
            ctx.font = fontSize + "px monospace";
            characterWidth = ctx.measureText("a").width;
            //draw background color
            ctx.fillStyle = Qt.rgba(0.1, 0.1, 0.1, 1);
            ctx.strokeStyle = Qt.rgba(0.5, 0.5, 0.5, 1);
            ctx.fillRect(0, 0, width, height);

            //draw secondary background
            ctx.fillStyle = Qt.rgba(0.3, 0.3, 0.3, 1);
            ctx.fillRect(0, 0, textBegin - 5, height);

            //draw cursor line
            if(drawCursor)
            {
                ctx.strokeStyle = Qt.rgba(1,1,1, 1);
                ctx.strokeRect(editorModel.cursorX * characterWidth + textBegin, y , 1 , lineHeight);
            }


            //draw box around text
            ctx.strokeStyle = Qt.rgba(0.5, 0.5, 0.5, 1);
            ctx.strokeRect(0, y, width, lineHeight);

            //Draw text
            var isComment = false;
            var isString = false;
            var Once = false;
            for(var i = 0; i < editorModel.lineCount(); i++)
            {
                ctx.fillStyle = Qt.rgba(0.8, 0.8, 0.8, 1);
                ctx.fillText(i + 1,10,lineHeight * (i + 1)- (lineHeight / 2) + 3);

                var offSet = textBegin;
                for(var j = 0; j < editorModel.tokensInLine(i); j++)
                {
                    var str = editorModel.text(i,j); // Getting the Word

                    if(Once) // just to color the last : " or ' in the Line
                        Once = false;

                    // Input Checker
                    if(str === "//")
                        isComment = true;
                    if(str === "\"" || str === "\'")
                        isString = !isString;

                    // Coloring the Text Based on the Input
                    ctx.fillStyle = editorModel.getColor(str);
                    if(isComment)
                        ctx.fillStyle = editorModel.getColor("comment");
                    if(isString)
                        ctx.fillStyle = editorModel.getColor("stringORchar");

                    // Fixing the color of the last : " or '
                    if(!isString && (str === "\"" || str === "\'"))
                    {
                        ctx.fillStyle = editorModel.getColor("stringORchar");
                        Once = true;
                    }

                    // Writing the Input
                    ctx.fillText(str, offSet, lineHeight * (i+ 1) - (lineHeight / 2) + 3);
                    offSet += ctx.measureText(str).width;
                }
                isComment = false;
                isString = false;

            }

        MouseArea {
            anchors.fill: parent
            cursorShape: Qt.IBeamCursor
            hoverEnabled: false
            onEntered: {}
            onExited: {}
            onWheel: {}
            onClicked: {
                var y = mouseY / lineHeight;
                var x = (mouseX - textBegin) / characterWidth;
                editorModel.mouseEventY(y);
                editorModel.mouseEventX(x);
                editorCanvas.requestPaint();
            }
        }

    }
    Keys.onPressed: {
        //console.debug(event.key);

        if(editorModel.keyEvent(event.text,event.key))
        {

            editorCanvas.requestPaint();
        }
    }

    EditorModel{
        id: editorModel
        Component.onCompleted: {
            //editorModel.init();
        }
    }
    Timer {
        interval: 500; running: true; repeat: true
        onTriggered:  {
            drawCursor = !drawCursor;
            editorCanvas.requestPaint();
        }
    }
}
