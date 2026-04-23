// authSync.js
// Sincroniza eventos de autenticação entre abas usando BroadcastChannel API.
// Suporte: Chrome 54+, Firefox 38+, Edge 79+, Safari 15.4+
// Carregado como módulo ES via import() dinâmico do CustomAuthStateProvider.

const AUTH_CHANNEL_NAME = 'goodhamburger_auth';
let channel = null;
let dotNetRef = null;

/**
 * Inicializa o listener de BroadcastChannel para esta aba.
 * @param {DotNetObjectReference} ref - Referência ao CustomAuthStateProvider desta aba.
 */
export function initAuthSync(ref) {
    // Fecha canal anterior se houver (segurança contra double-init)
    if (channel !== null) channel.close();

    dotNetRef = ref;
    channel = new BroadcastChannel(AUTH_CHANNEL_NAME);

    channel.onmessage = async (event) => {
        if (event.data?.type === 'logout') {
            // Notifica o circuito Blazor desta aba para executar o logout local
            await dotNetRef.invokeMethodAsync('OnRemoteLogout');
        }
    };
}

/**
 * Publica mensagem de logout para todas as outras abas abertas.
 * Chamado pela aba que originou o logout antes de limpar seus próprios tokens.
 */
export function broadcastLogout() {
    // Usa o canal existente ou cria um temporário se initAuthSync ainda não rodou
    const ch = channel ?? new BroadcastChannel(AUTH_CHANNEL_NAME);
    ch.postMessage({ type: 'logout' });
    if (!channel) ch.close(); // descarta o temporário
}

/**
 * Fecha o canal e libera recursos.
 * Chamado pelo DisposeAsync do CustomAuthStateProvider quando o circuito é destruído.
 */
export function disposeAuthSync() {
    channel?.close();
    channel = null;
    dotNetRef = null;
}
