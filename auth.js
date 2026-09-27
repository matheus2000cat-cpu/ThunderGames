// auth.js - Sistema de autenticação compartilhado + sincronização de UI

// IIFE (função autoexecutável) para não poluir o escopo global
(function () {
    // Chave no localStorage onde ficam os dados do usuário logado
    const STORAGE_USER_KEY = 'usuarioLogado';

    // Chave no localStorage onde fica o carrinho
    const STORAGE_CART_KEY = 'cart';

    // Lê o usuário logado do localStorage e converte de JSON para objeto
    function getLoggedUser() {
        try {
            // localStorage guarda strings, por isso precisamos fazer JSON.parse
            return JSON.parse(localStorage.getItem(STORAGE_USER_KEY));
        } catch {
            // Se der erro (JSON inválido, etc), retornamos null
            return null;
        }
    }

    // Verifica se existe usuário logado de verdade
    function isLoggedIn() {
        // Obtém o usuário atual
        const user = getLoggedUser();
        // Retorna true se user existir e tiver email
        return !!(user && user.email);
    }

    // Lê o carrinho do localStorage e garante que sempre retorne um array
    function getCart() {
        try {
            // Caso não exista ainda, usamos [] como padrão
            return JSON.parse(localStorage.getItem(STORAGE_CART_KEY)) || [];
        } catch {
            // Se houver falha ao parsear, carrinho vazio
            return [];
        }
    }

    // Calcula quantidade total de itens no carrinho (somando quantities)
    function getCartCount() {
        // Carrega o carrinho atual
        const cart = getCart();
        // Soma item.quantity (ou 1 se não existir) para obter o total
        return cart.reduce((sum, item) => sum + (item.quantity || 1), 0);
    }

    // Renderiza o contador do carrinho na UI (se existir o elemento)
    function renderCartCount() {
        // Procura o span/badge onde o número do carrinho aparece
        const cartCountEl = document.getElementById('cartCount');
        // Se não achou o elemento, não faz nada
        if (cartCountEl) {
            // Atualiza o texto com o total calculado
            cartCountEl.textContent = getCartCount();
        }
    }

    // Atualiza o botão de usuário (login vs perfil) com base no estado de autenticação
    function renderAuthUI() {
        // Procura o botão de usuário (ícone)
        const userBtn = document.getElementById('userBtn');
        // Se não existir na página atual, não faz nada
        if (!userBtn) return;

        // Se o usuário estiver logado
        if (isLoggedIn()) {
            // Muda a cor para destacar (usando CSS variable)
            userBtn.style.color = 'var(--primary)';

            // Ao clicar, vai para a página de perfil
            userBtn.onclick = () => (window.location.href = 'usuario.html');

            // Recupera os dados para usar no tooltip
            const user = getLoggedUser();

            // Tooltip com nome do usuário (quando existir)
            userBtn.title = user && user.nome ? `Olá, ${user.nome}! Ver perfil` : 'Ver perfil';
        } else {
            // Se não estiver logado, limpa cor customizada (volta ao padrão CSS)
            userBtn.style.color = '';

            // Ao clicar, vai para login
            userBtn.onclick = () => (window.location.href = 'login.html');

            // Tooltip informando para fazer login
            userBtn.title = 'Fazer Login';
        }
    }

    // Faz logout removendo usuário do localStorage e redireciona para login
    function logout() {
        // Remove apenas a chave do usuário logado
        localStorage.removeItem(STORAGE_USER_KEY);

        // Re-renderiza UI do header para refletir logout
        renderAuthUI();

        // Redireciona para a tela de login
        window.location.href = 'login.html';
    }

    // Bloqueia acesso se não estiver logado
    function requireAuth() {
        // Se não estiver logado, manda para login
        if (!isLoggedIn()) {
            window.location.href = 'login.html';
            // Retorna false para o chamador saber que o acesso foi bloqueado
            return false;
        }
        // Se estiver logado, libera e retorna true
        return true;
    }

    // Inicializa UI global (header + contador do carrinho) e cria listeners
    function initGlobalUI() {
        // Renderiza o estado do botão de usuário
        renderAuthUI();

        // Renderiza contador do carrinho
        renderCartCount();

        // Listener do evento 'storage' para sincronizar estado entre abas
        window.addEventListener('storage', (e) => {
            // Se a alteração foi no usuário logado, atualiza UI do usuário
            if (e.key === STORAGE_USER_KEY) {
                renderAuthUI();
            }
            // Se a alteração foi no carrinho, atualiza contador do carrinho
            if (e.key === STORAGE_CART_KEY) {
                renderCartCount();
            }
        });

        // Listener de 'pageshow' para lidar com bfcache (voltar/navegação do navegador)
        window.addEventListener('pageshow', (evt) => {
            // evt.persisted indica se a página veio do cache do navegador
            if (evt.persisted) {
                // Re-renderiza UI para garantir que não ficou desatualizado
                renderAuthUI();
                renderCartCount();
            }
        });
    }

    // Expõe funções para serem chamadas pelo código inline do HTML
    window.getLoggedUser = getLoggedUser; // retorna usuário logado (ou null)
    window.isLoggedIn = isLoggedIn; // retorna boolean
    window.logout = logout; // executa logout
    window.requireAuth = requireAuth; // valida login
    window.renderCartCount = renderCartCount; // atualiza contador
    window.renderAuthUI = renderAuthUI; // atualiza botão usuário
    window.initGlobalUI = initGlobalUI; // inicializa listeners + render

    // auto-init: quando o DOM estiver pronto, inicializa global UI (se ainda não iniciou)
    document.addEventListener('DOMContentLoaded', () => {
        // Evita chamar initGlobalUI() duas vezes em páginas que chamam manualmente
        if (!window.__globalUI_inited) {
            // Marca como iniciado
            window.__globalUI_inited = true;
            // Inicializa UI global
            initGlobalUI();
        }
    });
})();

