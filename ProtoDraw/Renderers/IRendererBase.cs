namespace DirectNXAML.Renderers
{
    public interface IRendererBase
    {
        public abstract void Initialize(uint _width, uint _height);
        public abstract void StartRendering();
        public abstract void StopRendering();
        public abstract bool Render();
    }
}