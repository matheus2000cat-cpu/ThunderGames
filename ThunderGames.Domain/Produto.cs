namespace ThunderGames.Domain
{
    public class Produto
    {
        public int IdProduto {get; set;}
        public string NomeProduto {get; set;} = string.Empty;
        public string Descricao {get; set;}
        public decimal ValorUnit {get; set;}
        public int EstMinimo {get; set;}
        public string Imagem {get; set;}
        public int FK_IdCategoria {get; set;}
        public int Fk_PessoaUserId {get; set;}

    }
}