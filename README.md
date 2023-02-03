# ProtoDraw with DirectN
A sample of DirectN

## usage
1. restore NuGet Libraries
2. Invoke in Debug mode. If you some trouble, please check the `Shader.hlsl` is copied to correct location and the code is ASCII.
3. select "Dx11Renderer" page located in left pane
4. There should be appeared triangle. This is initial data in data array.
5. (Don't forget to) ***Push [Draw Line]*** button on right pane. It start line add state. To stop the mode, push [Select] button.
6. every mouse press adds some vertex data (in float). The count of data is on right pane.
7. It intend to add a line (means two vertecies) every "Mouse Press - Mouse Move- Mouse Release" events but I cannot see them and the first triangles disappeared (I'm now checking because it should keep location previously).
A single vertex consist of 12 float numbers. One line is 24 float numbers.
Just press mouse, move mouse and release mouse cause Access violation error.

## code
- The renderer is located in `Renderers\Dx11Renderer.cs`. it has a `MapVertexData()` where cause memory access violation.
- The data provider is `Model\SimpleDrawLineManager.cs`. At the top of the code has `VertexData` property which should be mapped as vertec data in renderer.

##### I'm sorry for my terrible coding skill. I struggled to cut this out.
Please help me the way to use dynamic vertex array correctly.