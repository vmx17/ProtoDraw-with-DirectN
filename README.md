# ProtoDraw with DirectN
A sample of DirectN

## usage
1. open in VisualStudio 2022 and restore NuGet packages.
2. Invoke in Debug mode. If you hit trouble, please check the `Shader.hlsl` is copied to correct location and the code is in ASCII.
3. select "Dx11Renderer" page in navigation menu located in left pane.
4. There should be appeared triangle. This is initial data in vertex array. (primitive is "Line list", not Triangle)
5. (Don't forget to) ***Push [Draw Line]*** button on right pane. It start 'line add' state machine. To stop it, push [Select] button.
6. In "line add" state machine, every mouse_press adds some vertex data (in float). The count of data is on right pane.
7. It should add a line (means two vertecies) every "Mouse_Press - Mouse_Move- Mouse_Release" events but I cannot see them and the first triangles disappeared (I'm now checking because it should keep location previously).
A single vertex consist of 12 float numbers. One line is 24 float numbers.

## repro error
- (- (It is gettit is getting rare.) Ing rare.) In "line add" state machine, iteration to add line (press mouse, move mouse and release mouse) cause access violation error.
- does not appear added line. Once you change to another page, then return back, some white line appeas.

## known bug
- even though after successful update, the added data is incorrectly appears in vertex buffer. There are is matrix unmatch. So, if it comes to visible, the line appears different position (but in normalized area, inside view volume).
- changing windows size does not invoke swap chain resize. Repeating resizing action fix this.

- does not appear added line.
(known bug)
- even though after successful update, the added data is incorrectly appears in vertex buffer. There are is matrix unmatch. So, if it comes to visible, the line appears different position (but in normalized area, inside view volume). 

## code
- The renderer is located in `Renderers\Dx11Renderer.cs`. it has a `MapVertexData()` where cause memory access violation.
- The data provider is `Model\SimpleDrawLineManager.cs`. At the top of the code has `VertexData` property which should be mapped as vertex data in renderer.
- The current control center is `ViewModels\DirectNPageViewModel`.

##### I'm ashamed to show my terrible code. I struggled to cut this out.
Please help me the way to use dynamic vertex array correctly.
