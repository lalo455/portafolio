import urllib.request
import urllib.error
import concurrent.futures
import time
import random
import json

# ── CONFIGURACIÓN DE LA PRUEBA ───────────────────────────────────────────────
SERVER_IP = "localhost"
API_URL = f"http://{SERVER_IP}/apiSap/api/integracion/pedidos"

NUM_REQUESTS = 20       # Total de pedidos a enviar
CONCURRENT_USERS = 10   # Usuarios enviando simultáneamente

def send_order(index):
    dia_random = random.randint(10, 30)
    fecha = f"2026-09-{dia_random:02d}"
    timestamp = int(time.time())

    payload = {
        "CardCode": "C00001",
        "CardName": "CLIENTE DEMO",
        "Sociedad": "DEMO_COMPANY",
        "Usuario": "PruebaEstres",
        "DocDate": fecha,
        "DocDueDate": fecha,
        "U_NumPed": f"PED-STRESS-{timestamp}-{index}",  # Único por pedido para evitar deduplicación
        "Comments": f"pedido {index}",
        "Reference1": "",
        "DocCurrency": "",
        "DocRate": "",
        "SalesPersonCode": -1,
        "ShipToCode": "",
        "Address": "",
        "JournalMemo": "Orden de Venta",
        "DocumentStatus": "O",
        "DocType": "I",
        "Discount": 0,
        "DocumentLines": [
            {
                "ItemCode": "HI0001",
                "ItemDescription": "DEMO ITEM 5 KG",
                "Precio": 20,
                "PrecioLinea": 0,
                "PrecioUnitario": 20.00,
                "Quantity": 1,
                "Currency": "$",
                "PrtgDiscount": 0,
                "VatGroup": "B0",
                "Rate": "",
                "WarehouseCode": "AMP",
                "MetrosExtra": "METROS_EXTRA",
                "SalesPersonCode": -1,
                "LineStatus": "O",
                "TaxCode": ""
            }
        ]
    }

    json_bytes = json.dumps(payload).encode('utf-8')
    req = urllib.request.Request(API_URL, data=json_bytes, headers={'Content-Type': 'application/json'})
    start_time = time.time()
    
    try:
        with urllib.request.urlopen(req, timeout=10) as response:
            elapsed = time.time() - start_time
            body = response.read().decode('utf-8')
            data = json.loads(body) if body else {}
            return {
                "index": index,
                "status_code": response.status,
                "success": True,
                "ticketId": data.get("ticketId"),
                "fecha": fecha,
                "time": elapsed
            }
    except urllib.error.HTTPError as e:
        elapsed = time.time() - start_time
        body = e.read().decode('utf-8') if e.fp else ""
        return {
            "index": index,
            "status_code": e.code,
            "success": False,
            "error": "Duplicado" if e.code == 409 else (body[:100] if body else f"HTTP Error {e.code}"),
            "fecha": fecha,
            "time": elapsed
        }
    except Exception as e:
        elapsed = time.time() - start_time
        return {
            "index": index,
            "status_code": 0,
            "success": False,
            "error": str(e),
            "fecha": fecha,
            "time": elapsed
        }

if __name__ == "__main__":
    print("=" * 70)
    print(f"🚀 INICIANDO PRUEBA DE ESTRÉS EN EL API SAP MIDDLEWARE")
    print(f"📌 Servidor Destino: {API_URL}")
    print(f"📦 Total de Pedidos: {NUM_REQUESTS}")
    print(f"👥 Usuarios Concurrencia: {CONCURRENT_USERS}")
    print("=" * 70)

    start_total = time.time()
    results = []

    with concurrent.futures.ThreadPoolExecutor(max_workers=CONCURRENT_USERS) as executor:
        futures = [executor.submit(send_order, i + 1) for i in range(NUM_REQUESTS)]
        for future in concurrent.futures.as_completed(futures):
            res = future.result()
            results.append(res)
            if res["success"]:
                print(f"✅ Pedido #{res['index']:02d} | Fecha: {res['fecha']} | Ticket: {res['ticketId']} | Tiempo API: {res['time']:.3f}s")
            else:
                print(f"❌ Pedido #{res['index']:02d} | Fecha: {res['fecha']} | Error [{res['status_code']}]: {res['error']} | Tiempo: {res['time']:.3f}s")

    total_time = time.time() - start_total
    successful = [r for r in results if r["success"]]
    failed = [r for r in results if not r["success"]]
    avg_api_time = sum(r["time"] for r in results) / len(results) if results else 0

    print("\n" + "=" * 70)
    print("📊 RESUMEN FINAL DE LA PRUEBA")
    print("=" * 70)
    print(f"⏱️ Tiempo Total de la Prueba: {total_time:.2f} segundos")
    print(f"⚡ Peticiones por Segundo (RPS): {len(results) / total_time:.2f} req/sec")
    print(f"⏱️ Tiempo Promedio de Respuesta API: {avg_api_time * 1000:.1f} ms")
    print(f"✅ Exitosos (Encolados en Hangfire): {len(successful)} / {NUM_REQUESTS}")
    print(f"❌ Fallidos / Rechazados: {len(failed)} / {NUM_REQUESTS}")
    print("=" * 70)
