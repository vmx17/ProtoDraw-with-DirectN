# ProtoDraw with DirectN
A prototyping with DirectN using WinUI3 desktop for some draw application.

Very thanks to [Simon Mourier](https://github.com/smourier) san.

This program was made to [ask](https://github.com/smourier/DirectN/issues/32) about DirectN.

## usage
1. open in VisualStudio 2022 and restore NuGet packages.
2. Invoke in Debug mode. If you hit trouble, please check the `Shader.hlsl` is copied to correct location and the code is in ASCII.
3. select "Dx11Renderer" page in navigation menu located in left pane.
4. There should be appeared triangle. This is initial data in vertex array. (primitive is "Line list", not Triangle)
5. (Don't forget to) ***Push [Draw Line]*** button on right pane. It starts tiny 'line add' state machine. To stop it, push [Select] button.
6. In "line add" state machine, every mouse_press adds some vertex data (in float). The count of data is on right pane.
7. It should add a line (means two vertecies) every "Mouse_Press - Mouse_Move- Mouse_Release" events. Blue line shows it as rubber band and fixed line is in white.

## repro error
- Rare (fixed?). In "line add" state machine, iteration to add line (press mouse, move mouse and release mouse) cause access violation error.

## known bug
- even though after successful update, the added data is incorrectly appears in vertex buffer. There is matrix unmatch. (Left up (0,0) is tranlated center of the screen.)
- changing windows size does not invoke swap chain resize. Repeating resizing action fix this.

## issue
- (This is limit of DirectX, not DirectN.) In this program, a vertex contains 12 float number. (pos, nor, tex, color : 48byte) With this vertecies, the limit of dynamic vertex buffer seems about 14000 vertecies on Intel J4125 (8GB memory). A line takes 2 vertecies, then only about 7000 lines available. Need some memory operation, like that using static buffer. (aside from considering other draw primitive, buffers.)

## code
- The renderer is located in `Renderers\Dx11Renderer.cs`. it has a `MapVertexData()` where cause memory access violation.
- The data provider is `Model\SimpleDrawLineManager.cs`. At the top of the code has `VertexData` property which should be mapped as vertex data in renderer.
- The current control center is `ViewModels\DirectNPageViewModel`.
