/* ╔══════════════════════════════════════════════════════════════╗
   ║  LA TIENDITA DEL BARRIO — APP LOGIC v2                     ║
   ║  Catálogo + Chat Flotante + Voz + Carrito                   ║
   ╚══════════════════════════════════════════════════════════════╝ */

// ─── ELEMENTOS DOM ────────────────────────────────────────────
const chatMessages = document.getElementById('chat-messages');
const chatInput = document.getElementById('chat-input');
const btnSend = document.getElementById('btn-send');
const btnVoice = document.getElementById('btn-voice');
const btnAudioToggle = document.getElementById('btn-audio-toggle');
const btnStopVoice = document.getElementById('btn-stop-voice');
const btnOpenChat = document.getElementById('btn-open-chat');
const chatPanel = document.getElementById('chat-panel');
const cartPanel = document.getElementById('cart-panel');
const cartOverlay = document.getElementById('cart-overlay');
const cartItemsEl = document.getElementById('cart-items');
const cartEmptyEl = document.getElementById('cart-empty');
const cartFooter = document.getElementById('cart-footer');
const cartTotalAmount = document.getElementById('cart-total-amount');
const cartBadge = document.getElementById('cart-badge');
const typingIndicator = document.getElementById('typing-indicator');
const productGrid = document.getElementById('product-grid');
const catalogLoading = document.getElementById('catalog-loading');
const catalogEmpty = document.getElementById('catalog-empty');
const searchInput = document.getElementById('search-input');

// ─── ESTADO ───────────────────────────────────────────────────
let isLoading = false;
let isRecording = false;
let audioEnabled = true;  // Audio activado por defecto
let isChatOpen = false;
let chatInitialized = false;
let recognition = null;
let allProducts = [];
let currentCategory = 'all';
let searchTimeout = null;
let chatHistory = []; // Memoria conversacional multi-turn para Don Lalo
// Usar la IP actual en lugar de localhost para que funcione en el celular
const API_BASE = window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1' 
    ? 'http://localhost:5020' 
    : `http://${window.location.hostname}:5020`;
// ╔══════════════════════════════════════════════════════════════╗
// ║  INICIALIZACIÓN                                             ║
// ╚══════════════════════════════════════════════════════════════╝

document.addEventListener('DOMContentLoaded', () => {
    lucide.createIcons();
    setupSpeechRecognition();
    cargarCatalogo();
    actualizarCarrito();

    // Marcar botón de audio como activo por defecto
    btnAudioToggle.classList.add('active');
});

// ╔══════════════════════════════════════════════════════════════╗
// ║  CATÁLOGO DE PRODUCTOS (PÁGINA PRINCIPAL)                   ║
// ╠══════════════════════════════════════════════════════════════╣
// ║  Cargamos todos los productos de la API y los mostramos     ║
// ║  como tarjetas en un grid. Se pueden filtrar por categoría  ║
// ║  o buscar por nombre.                                       ║
// ╚══════════════════════════════════════════════════════════════╝

async function cargarCatalogo() {
    try {
        catalogLoading.classList.remove('hidden');
        catalogLoading.innerHTML = '<p style="text-align:center;padding:2rem">⏳ Cargando catálogo...</p>';
        const controller = new AbortController();
        const timeoutId = setTimeout(() => controller.abort(), 15000);
        const response = await fetch(`${API_BASE}/api/productos`, { signal: controller.signal });
        clearTimeout(timeoutId);
        if (!response.ok) throw new Error(`HTTP ${response.status}`);
        allProducts = await response.json();
        catalogLoading.classList.add('hidden');
        renderizarProductos(allProducts);
    } catch (error) {
        if (error.name === 'AbortError') {
            catalogLoading.innerHTML = '<p style="text-align:center;padding:2rem">⏱️ El servidor tardó mucho. ¿Está corriendo el WebApi en puerto 5020?</p>';
        } else {
            catalogLoading.innerHTML = '<p style="text-align:center;padding:2rem">❌ Error al cargar productos. ¿Está el WebApi corriendo?</p>';
        }
        console.error('Error cargando catálogo:', error);
    }
}

function renderizarProductos(productos) {
    productGrid.innerHTML = '';

    if (productos.length === 0) {
        catalogEmpty.classList.remove('hidden');
        lucide.createIcons({ nodes: [catalogEmpty] });
        return;
    }

    catalogEmpty.classList.add('hidden');

    productos.forEach((p, index) => {
        const card = document.createElement('div');
        card.className = 'product-card';
        card.style.animationDelay = `${index * 0.04}s`;

        const badges = [];
        if (p.AptoDiabeticos) badges.push('<span class="badge badge-diabetic">Apto Diabéticos</span>');
        if (p.CaloriasPorPorcion <= 100) badges.push('<span class="badge badge-low-cal">Bajo en Cal</span>');

        card.innerHTML = `
            <div class="product-card-img-container">
                ${badges.length > 0 ? `<div class="product-card-badges-top">${badges.join('')}</div>` : ''}
                <img class="product-card-img" 
                     src="${p.ImagenUrl || ''}" 
                     alt="${escapeHTML(p.Nombre)}"
                     loading="lazy"
                     onerror="this.src='data:image/svg+xml,<svg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 100 100%22><rect width=%22100%22 height=%22100%22 fill=%22%23161F2C%22/><text x=%2250%22 y=%2258%22 text-anchor=%22middle%22 font-size=%2236%22>🛒</text></svg>'"
                >
            </div>
            <div class="product-card-body">
                <div>
                    <div class="product-card-brand">${escapeHTML(p.Marca || '')}</div>
                    <div class="product-card-name" title="${escapeHTML(p.Nombre)}">${escapeHTML(p.Nombre)}</div>
                    <div class="product-card-presentation">${escapeHTML(p.Presentacion || '')}</div>
                </div>
                <div class="product-card-footer">
                    <div class="product-card-price">$${p.Precio.toFixed(2)}</div>
                    <button class="btn-add-to-cart" onclick="agregarDesdeCard(${p.IdProducto}, '${escapeHTML(p.Nombre)}')" aria-label="Agregar ${escapeHTML(p.Nombre)} al carrito">
                        <i data-lucide="plus"></i>
                        Agregar
                    </button>
                </div>
            </div>
        `;
        productGrid.appendChild(card);
    });

    lucide.createIcons({ nodes: [productGrid] });
}

// Mapa de alias de categorías (nombre UI → nombre en BD)
const CATEGORIA_MAP = {
    'all': null,
    'diabeticos': 'diabeticos',  // especial
    'Bebidas': 'Bebidas',
    'Lácteos': 'Lácteos',
    'Botanas': 'Botanas',
    'Panadería': 'Panadería',
    'Despensa': 'Despensa',
    'Salsas': 'Salsas',
    'Barras': 'Barras',
    'Pastelería': 'Pastelería',
    'Cereales': 'Cereales'
};

// Filtrar por categoría — 100% local, sin llamadas a API
function filtrarCategoria(categoria) {
    currentCategory = categoria;

    // Actualizar pills activas
    document.querySelectorAll('.pill').forEach(pill => {
        pill.classList.toggle('active', pill.dataset.category === categoria);
    });

    let filtered;
    if (categoria === 'all') {
        filtered = [...allProducts];
    } else if (categoria === 'recetas') {
        // Filtrar productos para cocinar y despensa básica
        filtered = allProducts.filter(p =>
            ['Carnes', 'Embutidos', 'Frutas y Verduras', 'Salsas'].includes(p.Categoria) ||
            (p.DescripcionIA && (
                p.DescripcionIA.includes('receta') || 
                p.DescripcionIA.includes('cocina') || 
                p.DescripcionIA.includes('comida') ||
                p.DescripcionIA.includes('desayuno') ||
                p.DescripcionIA.includes('chilaquiles')
            ))
        );
    } else if (categoria === 'Carnes y Embutidos') {
        filtered = allProducts.filter(p => p.Categoria === 'Carnes' || p.Categoria === 'Embutidos');
    } else if (categoria === 'diabeticos') {
        filtered = allProducts.filter(p => p.AptoDiabeticos);
    } else {
        // Filtro exacto por categoría
        filtered = allProducts.filter(p =>
            p.Categoria && p.Categoria.toLowerCase() === categoria.toLowerCase()
        );
    }

    // Aplicar búsqueda si hay texto en el campo
    const searchTerm = searchInput ? searchInput.value.trim().toLowerCase() : '';
    if (searchTerm) {
        filtered = filtered.filter(p =>
            p.Nombre.toLowerCase().includes(searchTerm) ||
            (p.Marca && p.Marca.toLowerCase().includes(searchTerm)) ||
            (p.Categoria && p.Categoria.toLowerCase().includes(searchTerm))
        );
    }

    renderizarProductos(filtered);
}

// Búsqueda de productos con debounce — filtra local sobre allProducts
function buscarProductos(term) {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        const lowerTerm = (term || '').toLowerCase().trim();
        if (!lowerTerm) {
            filtrarCategoria(currentCategory);
            return;
        }
        const filtered = allProducts.filter(p =>
            p.Nombre.toLowerCase().includes(lowerTerm) ||
            (p.Marca && p.Marca.toLowerCase().includes(lowerTerm)) ||
            (p.Categoria && p.Categoria.toLowerCase().includes(lowerTerm)) ||
            (p.Presentacion && p.Presentacion.toLowerCase().includes(lowerTerm))
        );
        renderizarProductos(filtered);
    }, 250);
}

// Agregar al carrito directamente desde la tarjeta
async function agregarDesdeCard(idProducto, nombre) {
    try {
        const response = await fetch(`${API_BASE}/api/carrito`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ idProducto: idProducto, cantidad: 1 })
        });

        if (response.ok) {
            await actualizarCarrito();
            abrirCarrito();  // ← abrir carrito automáticamente
            mostrarToast(`✅ ${nombre} agregado`);
        }
    } catch (error) {
        console.error('Error agregando al carrito:', error);
    }
}

// Toast notification
function mostrarToast(mensaje) {
    const toast = document.createElement('div');
    toast.style.cssText = `
        position: fixed; bottom: 100px; left: 50%; transform: translateX(-50%);
        background: #1A1A1A; color: white; padding: 12px 24px; border-radius: 12px;
        font-size: 15px; font-weight: 500; z-index: 9999;
        box-shadow: 0 8px 30px rgba(0,0,0,0.15);
        animation: msg-in 0.3s ease;
    `;
    toast.textContent = mensaje;
    document.body.appendChild(toast);
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transition = 'opacity 0.3s ease';
        setTimeout(() => toast.remove(), 300);
    }, 2000);
}

// ╔══════════════════════════════════════════════════════════════╗
// ║  CHAT FLOTANTE                                              ║
// ╠══════════════════════════════════════════════════════════════╣
// ║  Al abrir por primera vez:                                  ║
// ║  - Muestra mensaje de bienvenida con quick actions           ║
// ║  - Lee la bienvenida en voz alta si el audio está activo     ║
// ╚══════════════════════════════════════════════════════════════╝

function toggleChatPanel() {
    isChatOpen = !isChatOpen;

    if (isChatOpen) {
        chatPanel.classList.remove('hidden');
        btnOpenChat.classList.add('chat-open');

        // Primera vez que se abre → mostrar bienvenida
        if (!chatInitialized) {
            chatInitialized = true;
            mostrarBienvenida();
        }

        // Focus en el input
        setTimeout(() => chatInput.focus(), 300);
    } else {
        chatPanel.classList.add('hidden');
        btnOpenChat.classList.remove('chat-open');
    }
}

function mostrarBienvenida() {
    const welcomeText = '¡Qué onda vecino! 👋 Soy Don Lalo, el tendero de la tiendita. ¿Qué se te antoja cocinar hoy o qué te hace falta para el mandado?';

    const div = document.createElement('div');
    div.className = 'message assistant-message';
    div.innerHTML = `
        <div class="chat-avatar-xs"><i data-lucide="chef-hat"></i></div>
        <div class="message-content">
            <p><strong>${welcomeText}</strong></p>
            <div class="quick-actions">
                <button class="quick-action-btn" onclick="enviarSugerencia('¿Qué me recomiendas cocinar hoy?')">
                    <i data-lucide="utensils"></i>
                    <span>¿Qué cocino hoy?</span>
                </button>
                <button class="quick-action-btn" onclick="enviarSugerencia('Arma el combo para unos Chilaquiles Verdes con pollo')">
                    <i data-lucide="flame"></i>
                    <span>Combo Chilaquiles</span>
                </button>
                <button class="quick-action-btn" onclick="enviarSugerencia('Dame los ingredientes para un Guacamole botanero')">
                    <i data-lucide="sparkles"></i>
                    <span>Guacamole Botanero</span>
                </button>
                <button class="quick-action-btn" onclick="enviarSugerencia('Quiero ingredientes para unas Sincronizadas doraditas')">
                    <i data-lucide="sandwich"></i>
                    <span>Sincronizadas</span>
                </button>
                <button class="quick-action-btn" onclick="enviarSugerencia('¿Qué botanas y café tienes para la tarde?')">
                    <i data-lucide="coffee"></i>
                    <span>Café y antojito</span>
                </button>
                <button class="quick-action-btn" onclick="enviarSugerencia('¿Qué tengo en mi carrito?')">
                    <i data-lucide="shopping-bag"></i>
                    <span>Ver mi carrito</span>
                </button>
            </div>
        </div>
    `;
    chatMessages.appendChild(div);
    lucide.createIcons({ nodes: [div] });

    // Leer bienvenida en voz alta con tono cálido
    if (audioEnabled) {
        setTimeout(() => {
            hablarTexto('¡Qué onda vecino! Soy Don Lalo. ¿Qué se te antoja cocinar hoy o qué te hace falta para el mandado?');
        }, 500);
    }
}

// ╔══════════════════════════════════════════════════════════════╗
// ║  CHAT — ENVIAR Y RECIBIR MENSAJES                          ║
// ╚══════════════════════════════════════════════════════════════╝

async function enviarMensaje() {
    const texto = chatInput.value.trim();
    if (!texto || isLoading) return;

    agregarMensajeUsuario(texto);
    chatInput.value = '';
    chatInput.focus();

    await procesarMensaje(texto);
}

function enviarSugerencia(texto) {
    if (isLoading) return;

    // Si el chat no está abierto, abrirlo
    if (!isChatOpen) {
        toggleChatPanel();
        // Esperar a que se abra antes de enviar
        setTimeout(() => {
            agregarMensajeUsuario(texto);
            procesarMensaje(texto);
        }, 400);
        return;
    }

    agregarMensajeUsuario(texto);
    procesarMensaje(texto);
}

async function procesarMensaje(texto) {
    isLoading = true;
    btnSend.disabled = true;
    mostrarTyping(true);

    // AbortController con timeout de 30 segundos
    const controller = new AbortController();
    const timeoutId = setTimeout(() => controller.abort(), 30000);

    try {
        const response = await fetch(`${API_BASE}/api/chat`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ 
                mensaje: texto, 
                historial: chatHistory 
            }),
            signal: controller.signal
        });
        clearTimeout(timeoutId);

        if (!response.ok) {
            const errorData = await response.text();
            console.error('API Error:', response.status, errorData);
            throw new Error(`Error del servidor: ${response.status}`);
        }

        const data = await response.json();
        mostrarTyping(false);

        // Guardar turnos en el historial multi-turn para contexto continuo
        chatHistory.push({ role: 'user', text: texto });
        if (data.respuesta) {
            chatHistory.push({ role: 'model', text: data.respuesta });
        }
        if (chatHistory.length > 8) {
            chatHistory = chatHistory.slice(-8);
        }

        // ── Detectar si la respuesta implica acción de carrito ─────────
        const lowerText = texto.toLowerCase();
        const lowerResp = (data.respuesta || '').toLowerCase();
        const esAccionCarrito = ['agrega','quita','elimina','agregado','eliminado','carrito'].some(
            w => lowerText.includes(w) || lowerResp.includes(w)
        );

        // ── Mostrar productos en el GRID (no en el chat) ───────────────
        let hayProductos = false;
        if (data.productosActualizados && data.productosActualizados.length > 0) {
            hayProductos = true;
            // Actualizar grid con animación de highlight
            renderizarProductos(data.productosActualizados);
            resaltarGrid();
            // Scroll suave al catálogo solo si el chat NO es de carrito
            if (!esAccionCarrito) {
                setTimeout(() => window.scrollTo({ top: 0, behavior: 'smooth' }), 200);
            }
        }

        // ── Respuesta en el chat: breve, sin listar productos ──────────
        // Si Gemini ya mencionó los productos en texto, lo mostramos.
        // Si hay productos en pantalla, agregamos nota discreta.
        agregarMensajeAsistente(data.respuesta);
        if (hayProductos) {
            agregarNotaPantalla(`👆 Mostrando ${data.productosActualizados.length} resultado(s) arriba`);
        }

        // ── Acción de carrito: actualizar Y abrir el panel ─────────────
        if (esAccionCarrito) {
            await actualizarCarrito();
            abrirCarrito();  // ← panel se abre automáticamente
        }

        // ── Orden confirmada: animación + limpiar carrito ───────────
        if (data.ordenConfirmada) {
            await mostrarAnimacionOrden();
        }

        // ── Leer respuesta en voz alta ─────────────────────────────────
        if (audioEnabled) {
            hablarTexto(data.respuesta);
        }

    } catch (error) {
        clearTimeout(timeoutId);
        mostrarTyping(false);
        if (error.name === 'AbortError') {
            agregarMensajeAsistente('⏱️ La respuesta tardó más de 30 segundos. Intenta de nuevo en un momento.');
        } else if (error.message.includes('Failed to fetch') || error.message.includes('NetworkError')) {
            agregarMensajeAsistente('🔌 No se pudo conectar al servidor. Verifica que el WebApi esté corriendo en puerto 5020.');
        } else {
            agregarMensajeAsistente(`😔 Error: ${error.message}. Intenta de nuevo.`);
        }
        console.error('Error en chat:', error);
    } finally {
        isLoading = false;
        btnSend.disabled = false;
    }
}

// ─── RENDERIZADO DE MENSAJES ──────────────────────────────────

function agregarMensajeUsuario(texto) {
    const div = document.createElement('div');
    div.className = 'message user-message';
    div.innerHTML = `
        <div class="chat-avatar-xs"><i data-lucide="user"></i></div>
        <div class="message-content"><p>${escapeHTML(texto)}</p></div>
    `;
    chatMessages.appendChild(div);
    lucide.createIcons({ nodes: [div] });
    scrollToBottom();
}

function agregarMensajeAsistente(texto) {
    const div = document.createElement('div');
    div.className = 'message assistant-message';
    div.innerHTML = `
        <div class="chat-avatar-xs"><i data-lucide="chef-hat"></i></div>
        <div class="message-content">${formatearRespuesta(texto)}</div>
    `;
    chatMessages.appendChild(div);
    lucide.createIcons({ nodes: [div] });
    scrollToBottom();
}

function formatearRespuesta(texto) {
    if (!texto) return '<p>Sin respuesta</p>';

    let html = texto.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
    html = html.replace(/\*(.*?)\*/g, '<em>$1</em>');

    const paragraphs = html.split(/\n\n+/);
    let result = '';

    for (const paragraph of paragraphs) {
        const trimmed = paragraph.trim();
        if (!trimmed) continue;

        const lines = trimmed.split('\n');
        const isListBlock = lines.every(l => /^\s*[-•*]\s|^\s*\d+[.)]\s/.test(l.trim()) || l.trim() === '');

        if (isListBlock && lines.filter(l => l.trim()).length > 0) {
            result += '<ul>';
            for (const line of lines) {
                const cleaned = line.trim().replace(/^[-•*]\s*/, '').replace(/^\d+[.)]\s*/, '');
                if (cleaned) result += `<li>${cleaned}</li>`;
            }
            result += '</ul>';
        } else {
            result += `<p>${trimmed.replace(/\n/g, '<br>')}</p>`;
        }
    }

    return result || `<p>${html}</p>`;
}

function escapeHTML(str) {
    const div = document.createElement('div');
    div.textContent = str;
    return div.innerHTML;
}

function scrollToBottom() {
    requestAnimationFrame(() => { chatMessages.scrollTop = chatMessages.scrollHeight; });
}

function mostrarTyping(show) {
    if (show) {
        typingIndicator.classList.remove('hidden');
        lucide.createIcons({ nodes: [typingIndicator] });
        scrollToBottom();
    } else {
        typingIndicator.classList.add('hidden');
    }
}

// ╔══════════════════════════════════════════════════════════════╗
// ║  CARRITO                                                    ║
// ╚══════════════════════════════════════════════════════════════╝

function toggleCart() {
    const isOpen = cartPanel.classList.toggle('open');
    if (isOpen) {
        cartOverlay.classList.remove('hidden');
        actualizarCarrito();
    } else {
        cartOverlay.classList.add('hidden');
    }
}

async function actualizarCarrito() {
    try {
        const response = await fetch(`${API_BASE}/api/carrito`);
        const items = await response.json();

        if (items.length === 0) {
            cartEmptyEl.classList.remove('hidden');
            cartFooter.classList.add('hidden');
            cartBadge.classList.add('hidden');
            cartItemsEl.querySelectorAll('.cart-item').forEach(item => item.remove());
            return;
        }

        cartEmptyEl.classList.add('hidden');
        cartFooter.classList.remove('hidden');

        const totalItems = items.reduce((sum, i) => sum + i.Cantidad, 0);
        cartBadge.textContent = totalItems;
        cartBadge.classList.remove('hidden');

        const totalPagar = items.reduce((sum, i) => sum + i.Subtotal, 0);
        cartTotalAmount.textContent = `$${totalPagar.toFixed(2)}`;

        cartItemsEl.querySelectorAll('.cart-item').forEach(item => item.remove());

        items.forEach(item => {
            const div = document.createElement('div');
            div.className = 'cart-item';
            div.setAttribute('data-item-id', item.IdItem);  // para animación removing
            div.innerHTML = `
                <img class="cart-item-img" src="${item.ImagenUrl || ''}" alt="${escapeHTML(item.Nombre)}"
                     onerror="this.src='data:image/svg+xml,<svg xmlns=%22http://www.w3.org/2000/svg%22 viewBox=%220 0 100 100%22><text y=%22.9em%22 font-size=%2280%22>🛒</text></svg>'">
                <div class="cart-item-info">
                    <div class="cart-item-name">${escapeHTML(item.Nombre)}</div>
                    <div class="cart-item-details">${escapeHTML(item.Marca || '')} · ${escapeHTML(item.Presentacion || '')}</div>
                    <div class="cart-item-price-qty">
                        <div class="cart-item-price">$${item.Subtotal.toFixed(2)}</div>
                        <div class="cart-qty-controls">
                            <button onclick="cambiarCantidadCarrito(${item.IdProducto}, -1)" class="btn-qty" aria-label="Disminuir">-</button>
                            <span>${item.Cantidad}</span>
                            <button onclick="cambiarCantidadCarrito(${item.IdProducto}, 1)" class="btn-qty" aria-label="Aumentar">+</button>
                        </div>
                    </div>
                </div>
                <button class="cart-item-delete" onclick="eliminarItemCarrito(${item.IdItem})" aria-label="Eliminar">
                    <i data-lucide="trash-2"></i>
                </button>
            `;
            cartItemsEl.appendChild(div);
        });

        lucide.createIcons({ nodes: [cartItemsEl] });
    } catch (error) {
        console.error('Error al cargar carrito:', error);
    }
}

async function eliminarItemCarrito(idItem) {
    // Animación slide-out ANTES de eliminar (feedback en tiempo real)
    const itemEl = document.querySelector(`[data-item-id="${idItem}"]`);
    if (itemEl) {
        itemEl.classList.add('removing');
        await new Promise(r => setTimeout(r, 240));  // esperar animación CSS
    }
    try {
        await fetch(`${API_BASE}/api/carrito/${idItem}`, { method: 'DELETE' });
        await actualizarCarrito();
        abrirCarrito();
    } catch (error) {
        console.error('Error al eliminar item:', error);
        if (itemEl) itemEl.classList.remove('removing'); // revertir si falla
    }
}

async function cambiarCantidadCarrito(idProducto, delta) {
    try {
        const response = await fetch(`${API_BASE}/api/carrito`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ idProducto: idProducto, cantidad: delta })
        });
        if (response.ok) {
            await actualizarCarrito();
        }
    } catch (error) {
        console.error('Error cambiando cantidad:', error);
    }
}

// ╔══════════════════════════════════════════════════════════════╗
// ║  VOZ — WEB SPEECH API                                       ║
// ╠══════════════════════════════════════════════════════════════╣
// ║  - Speech-to-Text: micrófono → texto                        ║
// ║  - Text-to-Speech: respuesta → voz alta                     ║
// ║  - Botón DETENER: cancela la lectura actual                  ║
// ╚══════════════════════════════════════════════════════════════╝

function setupSpeechRecognition() {
    const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!SpeechRecognition) {
        if (btnVoice) btnVoice.style.display = 'none';
        return;
    }

    recognition = new SpeechRecognition();
    recognition.lang = 'es-MX';
    recognition.continuous = false;
    recognition.interimResults = false;

    recognition.onresult = (event) => {
        const transcript = event.results[0][0].transcript;
        chatInput.value = transcript;
        stopRecording();
        enviarMensaje();
    };

    recognition.onerror = () => stopRecording();
    recognition.onend = () => stopRecording();
}

function toggleVoice() {
    if (isRecording) stopRecording();
    else startRecording();
}

function startRecording() {
    if (!recognition || isLoading) return;
    // Asegurar que el chat esté abierto
    if (!isChatOpen) toggleChatPanel();
    isRecording = true;
    btnVoice.classList.add('recording');
    chatInput.placeholder = '🎤 Escuchando...';
    try { recognition.start(); } catch (e) { recognition.stop(); setTimeout(() => recognition.start(), 100); }
}

function stopRecording() {
    isRecording = false;
    if (btnVoice) btnVoice.classList.remove('recording');
    if (chatInput) chatInput.placeholder = 'Escribe o habla...';
    if (recognition) try { recognition.stop(); } catch (e) { }
}

// ─── TEXT-TO-SPEECH ───────────────────────────────────────────

function toggleAudio() {
    audioEnabled = !audioEnabled;
    btnAudioToggle.classList.toggle('active', audioEnabled);

    if (!audioEnabled) {
        detenerVoz();
    }
}

function hablarTexto(texto) {
    if (!('speechSynthesis' in window)) return;

    window.speechSynthesis.cancel();

    const textoLimpio = texto
        .replace(/\*\*(.*?)\*\*/g, '$1')
        .replace(/\*(.*?)\*/g, '$1')
        .replace(/<[^>]*>/g, '')
        .replace(/[#*_~`]/g, '');

    const utterance = new SpeechSynthesisUtterance(textoLimpio);
    // Ajustes para una voz más amigable
    utterance.rate = 0.95;
    utterance.pitch = 1.15; // Un poco más alto para sonar más amigable

    const voices = window.speechSynthesis.getVoices();
    
    // Buscar voces específicas que suenan más naturales (Google o Sabina en Windows)
    const naturalVoice = voices.find(v => v.name.includes('Google español') || v.name.includes('Sabina')) ||
                         voices.find(v => v.lang.startsWith('es-MX')) ||
                         voices.find(v => v.lang.startsWith('es'));
                         
    if (naturalVoice) utterance.voice = naturalVoice;

    // Mostrar botón de detener mientras habla
    utterance.onstart = () => {
        btnStopVoice.classList.remove('hidden');
    };

    utterance.onend = () => {
        btnStopVoice.classList.add('hidden');
    };

    utterance.onerror = () => {
        btnStopVoice.classList.add('hidden');
    };

    window.speechSynthesis.speak(utterance);
}

function detenerVoz() {
    if ('speechSynthesis' in window) {
        window.speechSynthesis.cancel();
    }
    btnStopVoice.classList.add('hidden');
}

// Cargar voces
if ('speechSynthesis' in window) {
    window.speechSynthesis.onvoiceschanged = () => { };
}

// ╔══════════════════════════════════════════════════════════════╗
// ║  HELPERS DE UX                                              ║
// ╚══════════════════════════════════════════════════════════════╝

// Abre el carrito si no está abierto (no lo cierra si ya está open)
function abrirCarrito() {
    if (!cartPanel.classList.contains('open')) {
        cartPanel.classList.add('open');
        cartOverlay.classList.remove('hidden');
    }
}

// Destella el grid de productos para que el usuario note los nuevos resultados
function resaltarGrid() {
    productGrid.style.transition = 'opacity 0.15s ease';
    productGrid.style.opacity = '0.3';
    requestAnimationFrame(() => {
        setTimeout(() => {
            productGrid.style.opacity = '1';
        }, 150);
    });
}

// Agrega una nota discreta en el chat
function agregarNotaPantalla(texto) {
    const div = document.createElement('div');
    div.style.cssText = `
        text-align: center;
        font-size: 11px;
        color: var(--text-3, #9CA3AF);
        padding: 3px 12px;
        margin: 0 0 6px;
        opacity: 0.85;
    `;
    div.textContent = texto;
    chatMessages.appendChild(div);
    scrollToBottom();
}

// Animación de orden confirmada: muestra ✅ en el carrito y lo vacía
async function mostrarAnimacionOrden() {
    // Abrir el carrito para que el usuario vea la animación
    abrirCarrito();
    await new Promise(r => setTimeout(r, 200));

    // Reemplazar contenido del carrito con pantalla de éxito
    const successHtml = `
        <div class="order-success">
            <div class="order-success-icon">
                <i data-lucide="check"></i>
            </div>
            <h3>¡Orden confirmada!</h3>
            <p>Tu pedido está en camino.<br>Pago contra entrega.</p>
        </div>
    `;
    cartItemsEl.innerHTML = successHtml;
    cartFooter.classList.add('hidden');
    cartEmptyEl.classList.add('hidden');
    cartBadge.classList.add('hidden');
    lucide.createIcons({ nodes: [cartItemsEl] });

    // Cerrar el carrito y volver al estado normal después de 2.5s
    await new Promise(r => setTimeout(r, 2500));
    await actualizarCarrito();  // ahora el carrito está vacío en BD
    // Cerrar el panel suavemente
    cartPanel.classList.remove('open');
    cartOverlay.classList.add('hidden');
}

// ╔══════════════════════════════════════════════════════════════╗
// ║  FUNCIONES GLOBALES (accesibles desde HTML onclick)         ║
// ╚══════════════════════════════════════════════════════════════╝
window.enviarSugerencia = enviarSugerencia;
window.toggleCart = toggleCart;
window.toggleChatPanel = toggleChatPanel;
window.toggleVoice = toggleVoice;
window.toggleAudio = toggleAudio;
window.detenerVoz = detenerVoz;
window.enviarMensaje = enviarMensaje;
window.eliminarItemCarrito = eliminarItemCarrito;
window.cambiarCantidadCarrito = cambiarCantidadCarrito;
window.filtrarCategoria = filtrarCategoria;
window.buscarProductos = buscarProductos;
window.agregarDesdeCard = agregarDesdeCard;
