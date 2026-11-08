DO $seed$
DECLARE
    seller_id integer;
BEGIN
    SELECT user_id
    INTO seller_id
    FROM public.pessoas
    ORDER BY user_id
    LIMIT 1;

    IF seller_id IS NULL THEN
        RAISE EXCEPTION 'Cadastre primeiro uma pessoa pela API em POST /api/Pessoa para associar os produtos.';
    END IF;

    INSERT INTO public.categorias (nome_categoria)
    SELECT seed.nome_categoria
    FROM (VALUES
        ('Periféricos'),
        ('Hardware'),
        ('Jogos'),
        ('Consoles')
    ) AS seed(nome_categoria)
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.categorias AS categoria
        WHERE lower(categoria.nome_categoria) = lower(seed.nome_categoria)
    );

    INSERT INTO public.produtos (
        nome_produto,
        descricao,
        valor_unit,
        est_minimo,
        imagem,
        fk_id_categoria,
        fk_pessoa_user_id
    )
    SELECT
        seed.nome_produto,
        seed.descricao,
        seed.valor_unit,
        1,
        seed.imagem,
        categoria.id_categoria,
        seller_id
    FROM (VALUES
        ('Mouse Viper Pro', 'Mouse gamer de alta precisão.', 459.99::numeric(10, 2), 'image/Mouse Viper Pro.webp', 'Periféricos'),
        ('RTX 4070 Ti', 'Placa de vídeo para jogos e criação.', 5299.00::numeric(10, 2), 'image/RTX 4070 Ti.webp', 'Hardware'),
        ('Elden Ring: Shadow of Erdtree', 'Edição para PlayStation 5.', 199.90::numeric(10, 2), 'image/Elden Ring - Collector''s Ed.jpg', 'Jogos'),
        ('PS5 Slim 1TB', 'Console PlayStation 5 Slim com SSD de 1 TB.', 3749.50::numeric(10, 2), 'image/PS5 Slim 1TB.webp', 'Consoles'),
        ('Teclado G-Pro', 'Teclado mecânico para jogos.', 899.90::numeric(10, 2), 'image/Teclado G-Pro.jpg', 'Periféricos'),
        ('GTA V - Premium Edition', 'Edição Premium de Grand Theft Auto V.', 82.50::numeric(10, 2), 'image/GTA V - Premium Edition.webp', 'Jogos'),
        ('Ryzen 7 7800X3D', 'Processador para computadores gamer.', 2395.15::numeric(10, 2), 'image/Ryzen 7 7800X3D.webp', 'Hardware'),
        ('Resident Evil 4 Remake', 'Remake do jogo Resident Evil 4.', 149.00::numeric(10, 2), 'image/Resident Evil 4 Remake.webp', 'Jogos'),
        ('Nintendo Switch', 'Console Nintendo Switch.', 2190.40::numeric(10, 2), 'image/Nintendo Switch.jpg', 'Consoles'),
        ('Headset Cloud II', 'Headset para jogos com áudio imersivo.', 555.00::numeric(10, 2), 'image/Headset Cloud II.png', 'Periféricos'),
        ('Cyberpunk 2077', 'Jogo de RPG de ação futurista.', 125.00::numeric(10, 2), 'image/Cyberpunk 2077.jpg', 'Jogos'),
        ('PC Gamer High-End', 'Computador gamer de alto desempenho.', 5490.99::numeric(10, 2), 'image/PC Gamer High-End.webp', 'Hardware')
    ) AS seed(nome_produto, descricao, valor_unit, imagem, nome_categoria)
    INNER JOIN public.categorias AS categoria
        ON lower(categoria.nome_categoria) = lower(seed.nome_categoria)
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.produtos AS produto
        WHERE produto.nome_produto = seed.nome_produto
    );
END
$seed$;
