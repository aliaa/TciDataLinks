function initGojs(divId, data) {

    for (var i = 0; i < data.Nodes.length; i++) {
        if (data.Nodes[i].loc) {
            data.Nodes[i].loc = new go.Point(data.Nodes[i].loc.x, data.Nodes[i].loc.y);
        }
    }

    var gojs = go.GraphObject.make;  // for conciseness in defining templates

    if ($("#" + divId).children().length == 0) {
        myDiagram =
            gojs(go.Diagram, divId,  // Diagram refers to its DIV HTML element by id
                {
                    maxSelectionCount: 1 // no more than 1 element can be selected at a time
                });
        // define the node template
        myDiagram.nodeTemplate =
            gojs(go.Node, "Auto",
                new go.Binding("location", "loc"),
                {
                    locationSpot: go.Spot.Center,
                    toEndSegmentLength: 30, fromEndSegmentLength: 30
                },
                gojs(go.Shape, "Rectangle",
                    {
                        name: "OBJSHAPE",
                        fill: "white",
                        desiredSize: new go.Size(160, 30)
                    }),
                gojs(go.TextBlock,
                    { margin: 4 },
                    new go.Binding("text", "text")),
                {
                    toolTip:  //  define a tooltip for each node that displays its information
                        gojs("ToolTip",
                            gojs(go.TextBlock, { margin: 4 },
                                new go.Binding("text", "", getInfo))
                        )
                }
            );

        // define the link template
        myDiagram.linkTemplate =
            gojs(go.Link,
                {
                    selectionAdornmentTemplate:
                        gojs(go.Adornment,
                            gojs(go.Shape,
                                { isPanelMain: true, stroke: "dodgerblue", strokeWidth: 3 })
                            //gojs(go.Shape,
                            //  { toArrow: "Standard", fill: "dodgerblue", stroke: null, scale: 1 })
                        ),
                    routing: go.Link.Normal,
                    curve: go.Link.Bezier,
                    toShortLength: 2
                },
                gojs(go.Shape,  //  the link shape
                    { name: "OBJSHAPE", strokeWidth: 2 },
                    new go.Binding("stroke", "color")),
                //gojs(go.Shape,  //  the arrowhead
                //  { name: "ARWSHAPE", toArrow: "Standard" }),
                {
                    toolTip:  //  define a tooltip for each link that displays its information
                        gojs("ToolTip",
                            gojs(go.TextBlock, { margin: 4, stroke: 'red' },
                                new go.Binding("text", "text"))
                        )
                }
            );

        // define the group template
        myDiagram.groupTemplate =
            gojs(go.Group, "Spot",
                {
                    selectionAdornmentTemplate: // adornment when a group is selected
                        gojs(go.Adornment, "Auto",
                            gojs(go.Shape, "Rectangle",
                                { fill: null, stroke: "dodgerblue", strokeWidth: 3 }),
                            gojs(go.Placeholder)
                        ),
                    toSpot: go.Spot.AllSides, // links coming into groups at any side
                    toEndSegmentLength: 30, fromEndSegmentLength: 30
                },
                gojs(go.Panel, "Auto",
                    gojs(go.Shape, "Rectangle",
                        {
                            name: "OBJSHAPE",
                            parameter1: 14,
                            fill: "rgba(0,0,0,0.09)"
                        },
                        new go.Binding("desiredSize", "ds")),
                    gojs(go.Placeholder,
                        { padding: 28 })
                ),
                gojs(go.TextBlock,
                    {
                        name: "GROUPTEXT",
                        alignment: go.Spot.TopLeft,
                        alignmentFocus: new go.Spot(0, 0, -4, -4),
                        font: "Bold 10pt 'Segoe UI'"
                    },
                    new go.Binding("text", "text")),
                {
                    toolTip:  //  define a tooltip for each group that displays its information
                        gojs("ToolTip",
                            gojs(go.TextBlock, { margin: 4 },
                                new go.Binding("text", "", getInfo))
                        )
                }
            );
    }

    // add nodes, including groups, and links to the model
    myDiagram.model = new go.GraphLinksModel(data["Nodes"], data["Links"]);


    //myDiagram.select(myDiagram.findNodeForKey('C'));

    myDiagram.addDiagramListener("ObjectDoubleClicked",
        function (e) {
            var part = e.subject.part;
            if (part instanceof go.Link) {
                window.location = "/Connection/Item/" + part.data.connectionId;
            }
            else {
                var split = part.data.key.split("_");
                if (split[0] == "Device") {
                    window.location = "/Device/Item/" + split[1];
                }
                else if (split[0] == "Passive") {
                    window.location = "/Passive/Item/" + split[1];
                }
                else {
                    window.location = "/Place/Item/" + split[1] + "?type=" + split[0];
                }
            }
        });
    return myDiagram;
}


// returns the text for a tooltip, param obj is the text itself
function getInfo(model, obj) {
    var data = obj.panel.adornedPart.data;
    if (data.text)
        return data.text;
    return data.from + " --> " + data.to;
}