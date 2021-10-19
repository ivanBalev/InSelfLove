namespace BDInSelfLove.Web.InputModels.Article
{
    public class ArticleEditInputModel : ArticleCreateInputModel
    {
        public int Id { get; set; }

        public string Slug => this.Title.ToLower().Replace(' ', '-');
    }
}
