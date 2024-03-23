const spawnPos = [686.245, 577.950, 130.461];

on('onClientGameTypeStart', () => {
  exports.spawnmanager.setAutoSpawnCallback(() => {
    exports.spawnmanager.spawnPlayer({
      x: spawnPos[0],
      y: spawnPos[1],
      z: spawnPos[2],
      model: 'a_m_m_skater_01'
    }, () => {
      emit('chat:addMessage', {
        args: [
          'Welcome to the party!~'
        ]
      })
    });
  });

  exports.spawnmanager.setAutoSpawn(true)
  exports.spawnmanager.forceRespawn()
});


RegisterCommand('myjs', (source, args, raw) => {
    // TODO: make a vehicle! fun!
    emit('chat:addMessage', {
      args: [`I wish I could spawn this ${(args.length > 0 ? `${args[0]} or` : ``)} adder but my owner was too lazy. :(`]
    });
  }, false);
  