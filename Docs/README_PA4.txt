PRODUCTO ACADEMICO 4 - CAPSTONE 3D

Juego: Ruinas del Bosque Olvidado
Genero: exploracion / escape 3D en un bosque nocturno con ruinas antiguas.

HISTORIA: eres una exploradora atrapada en unas ruinas ancestrales. Un espiritu del bosque
vigila el lugar. Debes recuperar 3 runas ancestrales para despertar el portal de luz verde y escapar.

OBJETIVO: recoger 3 runas ancestrales y cruzar el portal ancestral.
RECORRIDO: claro -> sendero -> plaza (fuente, runa 1, espiritu) -> torre este (runa 2) -> puente con salto -> santuario (runa 3 en el altar) -> portal.
CONTROLES: WASD o flechas para mover, ESPACIO para saltar, ESC para pausar, R para reiniciar al terminar.

Sistemas implementados:
- Entorno 3D de bosque: arboles, ruinas de piedra, columnas rotas, altar con monolito, hongos luminosos,
  luciernagas animadas, luz de luna, niebla verdosa y colliders.
- Jugador (Exploradora con capucha, mochila y farol) con CharacterController, movimiento y salto.
- Camara en tercera persona.
- UI con runas, vidas y tiempo.
- Audio ambiental (pad suave + viento) y efectos generados por C#.
- Particulas en las runas y efectos de recoleccion.
- Enemigo Espiritu_Guardian (espiritu con astas de ciervo) con maquina de estados:
  patrulla -> alerta -> persecucion -> aturdido (se disipa tras golpear).
- Caida al vacio: pierdes una vida y reapareces al inicio.
- Condicion de victoria/derrota.
- Object Pooling para efectos de particulas.
- AI optimizada: decision cada 0.15 segundos en lugar de recalcular deteccion cada frame.

El nivel es corto (3-7 minutos) y prioriza estabilidad e integracion, tal como recomienda la consigna.
