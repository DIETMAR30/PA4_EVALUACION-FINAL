# Guía de pruebas - PA4

Estas pruebas sirven para la captura de evidencias del informe y la sustentación.

### Prueba 01 - Inicio y movimiento
1. Ejecutar la escena.
2. Mover a la exploradora con W, A, S y D.
3. Comprobar cámara en tercera persona y el farol que ilumina el entorno.
4. Presionar Espacio y verificar el salto.

**Evidencia sugerida:** captura del gameplay mostrando HUD, árboles y ruinas.

### Prueba 02 - Recolección
1. Llegar a la primera runa (plaza de las columnas, vigilada por el espíritu).
2. Subir por la rampa a la torre este para la segunda runa.
3. Cruzar el puente de raíces (hay un hueco que exige saltar), subir al santuario y saltar sobre el altar para la tercera.
4. Confirmar que el HUD cambia de 0/3 a 3/3.

**Evidencia sugerida:** captura con `RUNAS 3/3`.

### Prueba 03 - IA (estados del espíritu)
1. Entrar en la plaza de las columnas.
2. Observar al Espiritu_Guardian mientras flota por sus waypoints (patrulla lenta).
3. Entrar en su radio de detección (9 m): el espíritu se detiene y aparece el aviso (estado de alerta, 0.7 s).
4. Confirmar que después persigue al jugador.
5. Alejarse más de 13 m y confirmar que pierde el rastro y vuelve a patrullar.

**Evidencia sugerida:** captura de la exploradora y el espíritu en la misma escena.

### Prueba 04 - Victoria
1. Reunir las tres runas.
2. Cruzar el portal de luz verde.
3. Verificar el mensaje de escape completado y el tiempo final.

### Prueba 05 - Derrota y reinicio
1. Permitir que el espíritu alcance a la exploradora hasta agotar las vidas.
2. Verificar que tras cada golpe el espíritu se disipa unos segundos y la exploradora vuelve al inicio.
3. Verificar el mensaje de derrota.
4. Pulsar R y comprobar que la partida comienza nuevamente.

### Prueba 06 - Caída al vacío
1. Saltar fuera de una plataforma o fallar el salto del puente hacia el abismo.
2. Verificar que se pierde una vida y la exploradora reaparece en el claro inicial.

### Prueba 07 - Optimización
Durante la sustentación explicar que los efectos de impacto utilizan `ImpactPool` y que la IA toma decisiones cada 0.15 segundos en lugar de evaluar la detección completa en cada frame.
