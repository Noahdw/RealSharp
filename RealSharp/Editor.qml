import QtQuick 2.9
import QtQuick.Layouts 1.3
import QtQuick.Controls 2.4
import QtQuick.Controls.Material 2.1
import Editor 1.0

Rectangle {
    focus: true
    property int lineHeight: 16
    property int textBegin: 35
    property int characterWidth: 8
    property int fontSize: 14
    property int initX: 0
    property int initY: 0
    property bool drawCursor: true
    property bool canSelect: false
    property bool firstRun: true
    width: parent.width
    height: parent.height
    color: "black"
    Component.onCompleted: {
        if (firstRun) // dumb wrokaround, gets called 3 times ???
            editorModel.init();
        firstRun = false;
    }
    Canvas {
        id: editorCanvas
        width: parent.width
        height: parent.height
        renderStrategy: Canvas.Threaded
        onPaint: {
            var ctx = getContext("2d");
            var yCursorHeight = editorModel.cursorY * lineHeight;
            ctx.font = fontSize + "px monospace";
            characterWidth = ctx.measureText("a").width;

            ctx.clearRect(0,0,width,height);
            // draw secondary background
            ctx.fillStyle = Qt.rgba(0.3, 0.3, 0.3, 1);
            ctx.fillRect(0, 0, textBegin - 5, height);

            // ==== DRAW HIGHLIGHT SELECTION ==== //
            ctx.fillStyle = "#5792f2";
            // TODO : Pre-rendering broke this
            if (canSelect)
            {
                var linesToDraw = Math.abs(initY - editorModel.cursorY);
                for (var j = 0; j <= linesToDraw; j++)
                {
                    var direction = initY - editorModel.cursorY < 0 ? 1 : -1;
                    var charactersInLine = editorModel.charactersInLine(initY + j * direction);
                    var lineWidth = charactersInLine;
                    var selectStart = 0;
                    if (j === 0)
                    {
                        lineWidth = editorModel.cursorX > charactersInLine ? charactersInLine : initX; // draw from xcursor to the left on inital clicked line
                        if (linesToDraw === 0) // special behavior if only one line selected
                        {
                            selectStart = editorModel.cursorX * characterWidth;
                            lineWidth = (initX - editorModel.cursorX);
                        }
                        else if (direction === 1)
                        {
                            selectStart = initX * characterWidth;
                            lineWidth = charactersInLine - initX
                        }
                    }
                    else if (initY + j * direction === editorModel.cursorY)
                    {
                        if (direction === 1)
                        {
                            lineWidth = editorModel.cursorX;
                        }
                        else
                        {
                            selectStart = editorModel.cursorX * characterWidth;
                            lineWidth = charactersInLine - editorModel.cursorX;
                        }
                    }
                    ctx.fillRect(textBegin + selectStart, (initY + j * direction) * lineHeight, lineWidth * characterWidth, lineHeight);
                }
            }

            // draw cursor line
            if (drawCursor)
            {
                ctx.strokeStyle = Qt.rgba(1, 1, 1, 1);
                ctx.strokeRect(editorModel.cursorX * characterWidth + textBegin, yCursorHeight, 1, lineHeight);
            }

            // draw box around text
            ctx.strokeStyle = Qt.rgba(0.5, 0.5, 0.5, 1);
            ctx.strokeRect(0, yCursorHeight, width, lineHeight);

            // === DRAW ALL DIRTIED / NEW TEXT === //
            if (editorModel.textRedrawNeeded)
            {
                var offCtx = offScreenCanvas.getContext("2d");
                
                offCtx.font = fontSize + "px monospace";
                // var list = editorModel.LinesToDraw; // DOES NOT WORK :()
                // var toDraw = Net.toListModel(list);
                for (var k = 0; k < editorModel.numLinesToDraw(); k++)
                {
                    // Last I tried, Qml.Net didn't support directly accessing a .Net List,
                    // So I had to create helper methods for accessing some of its functions.
                    // TODO: Check if support was added. I think it was.
                    var lineNumber = editorModel.getLineToDraw(k);

                    // === DRAW LINE NUMBERS === //
                    offCtx.fillStyle = Qt.rgba(0.8, 0.8, 0.8, 1);
                    offCtx.fillText(lineNumber + 1, 10, lineHeight * (lineNumber + 1) - (lineHeight / 2) + 3);

                    var offSet = textBegin;
                    var tokensInLine = editorModel.tokensInLine(lineNumber);
                    // clear the line rect to be drawn
                    offCtx.clearRect(offSet, lineHeight * (lineNumber), width, lineHeight);

                    for (var j = 0; j < tokensInLine; j++)
                    {
                        var token = editorModel.text(lineNumber, j);

                        var isComment = false;
                        var isQuotedText = false;

                        // Input Checker
                        if (token === "//")
                        {
                            isComment = true;
                        }
                        else if (token === "\"" || token === "\'")
                        {
                            isQuotedText = !isQuotedText;
                        }

                        // Coloring the Text Based on the Input
                        if (isComment) 
                        {
                            offCtx.fillStyle = editorModel.getColor("comment");
                        }
                        else if (isQuotedText)
                        {
                            offCtx.fillStyle = editorModel.getColor("stringORchar");
                        }
                        else
                        {
                            offCtx.fillStyle = editorModel.getColor(token);
                        }

                        // Fixing the color of the last : " or '
                        if (!isQuotedText && (token === "\"" || token === "\'"))
                        {
                            offCtx.fillStyle = editorModel.getColor("stringORchar");
                        }

                        // Writing the Input
                        offCtx.fillText(token, offSet, lineHeight * (lineNumber + 1) - (lineHeight / 2) + 3);
                        offSet += offCtx.measureText(token).width;
                    }
                }
                editorModel.textRedrawNeeded = false;
                editorModel.clearLinesToDraw();
            }
            ctx.drawImage(offScreenCanvas, 0, 0);
        }
        Canvas{
            id: offScreenCanvas
            width: parent.width
            height: parent.height
            renderStrategy: Canvas.Threaded
        }

        MouseArea {
            anchors.fill: parent
            cursorShape: Qt.IBeamCursor
            hoverEnabled: false
            onEntered: {}
            onExited: {}
            onWheel: {}
            onPressed: {
                canSelect = true;
                var y = mouseY / lineHeight;
                var x = (mouseX - textBegin) / characterWidth;
                editorModel.HandleMouseEventY(y);
                editorModel.HandleMouseEventX(x);
                initX = editorModel.cursorX;
                initY = editorModel.cursorY;
                editorCanvas.requestPaint();
            }
            onMouseXChanged: {
                var x = (mouseX - textBegin) / characterWidth;
                editorModel.HandleMouseEventX(x);
                editorCanvas.requestPaint();
            }
            onMouseYChanged: {
                var y = mouseY / lineHeight;
                editorModel.HandleMouseEventY(y);
                editorCanvas.requestPaint();
            }
            onReleased: {}
        }
    }
    // Obviously this will need major changes to allow for supporting changing keybinds
    Keys.onPressed: {
        if (event.modifiers)
        {
            console.debug(event.key);
            if (event.modifiers === Qt.ControlModifier)
            {
                if (canSelect && event.key === Qt.Key_C)
                {
                    editorModel.copyText(initX,initY);
                }
                else if (event.key === Qt.Key_V)
                {
                    editorModel.pasteText();
                    canSelect = false;
                    editorCanvas.requestPaint();
                }
                else if (event.key === Qt.Key_Z)
                {
                    editorModel.undoCommand();
					editorCanvas.requestPaint();
                }
                else if (event.key === Qt.Key_D)
                {
                    editorModel.printDebugInfo();
                }
                return;
            }
        }
        if (editorModel.keyEvent(event.text, event.key))
        {
            editorCanvas.requestPaint();
        }

        if (event.key === Qt.Key_Tab) 
        {
            event.accepted = true;
        }
        canSelect = false;
    }

    EditorModel{
        id: editorModel
    }
    Timer {
        interval: 500; running: true; repeat: true
        onTriggered: {
            drawCursor = !drawCursor;
            editorCanvas.requestPaint();
        }
    }
}
