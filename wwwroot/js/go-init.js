function initGojs(divId, data) {
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
                            gojs(go.TextBlock, { margin: 4 },
                                new go.Binding("text", "text", getInfo))
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
                            fill: "rgba(0,200,100,0.1)"
                        },
                        new go.Binding("desiredSize", "ds")),
                    gojs(go.Placeholder,
                        { padding: 32 })
                ),
                gojs(go.TextBlock,
                    {
                        name: "GROUPTEXT",
                        alignment: go.Spot.TopLeft,
                        alignmentFocus: new go.Spot(0, 0, -4, -4),
                        font: "Bold 10pt Sans-Serif"
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
                else if (split[0] == "PatchPanel") {
                    window.location = "/PatchPanel/Item/" + split[1];
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
    var x = obj.panel.adornedPart; // the object that the mouse is over
    var text = ""; // what will be displayed
    if (x instanceof go.Node) {
        if (x instanceof go.Group) text += "Group: "; else text += "Node: ";
        text += x.data.key;
        var toLst = nodesTo(x, 0); // display names of nodes going into this node
        if (toLst.count > 0) {
            toLst.sort(function (a, b) { return a < b ? -1 : 1 });
            text += "\nNodes into: ";
            toLst.each(function (key) {
                if (key !== text.substring(text.length - 3, text.length - 2)) {
                    text += key + ", ";
                }
            });
            text = text.substring(0, text.length - 2);
        }
        var frLst = nodesFrom(x, 0); // display names of nodes coming out of this node
        if (frLst.count > 0) {
            frLst.sort(function (a, b) { return a < b ? -1 : 1 });
            text += "\nNodes out of: ";
            frLst.each(function (key) {
                if (key !== text.substring(text.length - 3, text.length - 2)) {
                    text += key + ", ";
                }
            });
            text = text.substring(0, text.length - 2);
        }
        var grpC = containing(x, 0); // if the node is in a group, display its name
        if (grpC !== null) text += "\nContaining SubGraph: " + grpC.data.key;
        if (x instanceof go.Group) {
            // if it"s a group, also display nodes and links contained in it
            text += "\nMember nodes: ";
            var children = childNodes(x, 0);
            children.sort(function (a, b) { return a < b ? -1 : 1 });
            children.each(function (key) {
                if (key !== text.substring(text.length - 3, text.length - 2)) {
                    text += key + ", ";
                }
            });
            text = text.substring(0, text.length - 2);

            var linkChildren = childLinks(x, 0);
            if (linkChildren.count > 0) {
                text += "\nMember links: ";
                var linkStrings = new go.List(/*"string"*/);
                linkChildren.each(function (link) {
                    linkStrings.add(link.data.from + " --> " + link.data.to);
                });
                linkStrings.sort(function (a, b) { return a < b ? -1 : 1 });
                linkStrings.each(function (str) {
                    text += str + ", ";
                });
                text = text.substring(0, text.length - 2);
            }
        }
    } else if (x instanceof go.Link) {
        // if it"s a link, display its to and from nodes
        text += "Link: " + x.data.from + " --> " + x.data.to +
            "\nNode To: " + x.data.to + "\nNode From: " + x.data.from;
        var grp = containing(x, 0); // and containing group, if it has one
        if (grp !== null) text += "\nContaining SubGraph: " + grp.data.key;
    }
    return text;
}