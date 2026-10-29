using HearthRemedies.Model;

namespace HearthRemedies.Controller
{
    public enum PostSort { Newest, Top }

    public class PostFilterState
    {
        public RemedyType? Type { get; private set; }
        public RemedyMethod? Method { get; private set; }
        public PostSort Sort { get; private set; } = PostSort.Newest;

        public bool IsFiltered => Type is not null || Method is not null || Sort != PostSort.Newest;

        public event Action? OnChange;

        public void SetType(RemedyType? type) { Type = type; OnChange?.Invoke(); }
        public void SetMethod(RemedyMethod? method) { Method = method; OnChange?.Invoke(); }
        public void SetSort(PostSort sort) { Sort = sort; OnChange?.Invoke(); }

        public void Clear()
        {
            Type = null;
            Method = null;
            Sort = PostSort.Newest;
            OnChange?.Invoke();
        }
    }
}