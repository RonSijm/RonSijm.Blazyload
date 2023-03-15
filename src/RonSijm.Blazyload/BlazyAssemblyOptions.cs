namespace RonSijm.Blazyload
{
    public class BlazyAssemblyOptions
    {
        /// <summary>
        /// Lets you define a custom class path
        /// </summary>
        public string ClassPath { get; set; }

        /// <summary>
        /// Disables CascadeLoading for a specific assembly
        /// </summary>
        public bool DisableCascadeLoading { get; set; }
    }
}
