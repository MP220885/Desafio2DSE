param([string]$Gateway = 'http://localhost:5000')

$ErrorActionPreference = 'Stop'
$credenciales = @{ usuario = 'admin'; clave = 'desafio2' } | ConvertTo-Json
$login = Invoke-RestMethod "$Gateway/auth/token" -Method Post -ContentType 'application/json' -Body $credenciales
$headers = @{ Authorization = "Bearer $($login.access_token)" }

$clientesExistentes = @((Invoke-RestMethod "$Gateway/clientes" -Headers $headers) | Where-Object { $null -ne $_ })
if ($clientesExistentes.Count -gt 0) {
    throw 'La siembra requiere una base de clientes vacia. Elimine clientes.db, reinicie las APIs y vuelva a ejecutar el script.'
}

$idsClientes = [System.Collections.Generic.List[int]]::new()
for ($n = 1; $n -le 1500; $n++) {
    Write-Progress -Activity 'Creando clientes' -Status "$n de 1500" -PercentComplete (($n / 1500) * 100)
    $cliente = @{ nombre = "Cliente $n"; email = "cliente$n@prueba.local"; telefono = "7000-$('{0:d4}' -f $n)"; direccion = "Direccion $n" } | ConvertTo-Json
    $creado = Invoke-RestMethod "$Gateway/clientes" -Method Post -Headers $headers -ContentType 'application/json' -Body $cliente
    $idsClientes.Add($creado.id)
}

for ($n = 1; $n -le 1500; $n++) {
    Write-Progress -Activity 'Creando pedidos' -Status "$n de 1500" -PercentComplete (($n / 1500) * 100)
    $pedido = @{ clienteId = $idsClientes[$n - 1]; descripcion = "Pedido de prueba $n"; total = [math]::Round((($n * 17) % 900) + 10.5, 2); estado = 'Pendiente' } | ConvertTo-Json
    Invoke-RestMethod "$Gateway/pedidos" -Method Post -Headers $headers -ContentType 'application/json' -Body $pedido | Out-Null
}

Write-Progress -Activity 'Siembra de datos' -Completed
Write-Host 'Siembra finalizada correctamente: 1,500 clientes y 1,500 pedidos.'
