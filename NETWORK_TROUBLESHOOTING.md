# 🌐 Terminal Racer - Network Multiplayer Troubleshooting Guide

Complete guide to diagnose and fix network connection issues between Kisumu and Nakuru.

## 🔍 Connection Timeout Issue (Your Current Problem)

**Symptom**: Client shows "Timeout" on all 5 attempts when connecting to host

### Root Causes & Solutions

#### 1. **Host Not Actually Listening** (Most Common)
**Check on Host Machine:**
```bash
# Verify port 9999 is listening
netstat -tuln | grep 9999
# or
ss -tuln | grep 9999
```

**Expected output:**
```
tcp        0      0 0.0.0.0:9999            0.0.0.0:*               LISTEN
```

**If NOT listening:**
- Host game crashed silently
- Host is still in menu, not started server
- Port is already in use by another application

**Solution:**
```bash
# Kill any process using port 9999
sudo lsof -i :9999
sudo kill -9 <PID>

# Then restart host game
```

---

#### 2. **Firewall Blocking Port 9999**
**Check on Host Machine (Linux):**
```bash
# Check UFW status
sudo ufw status

# If enabled, verify port 9999 is allowed
sudo ufw allow 9999/tcp

# Check iptables
sudo iptables -L -n | grep 9999
```

**Check on Host Machine (Windows):**
```powershell
# Check Windows Firewall
netsh advfirewall firewall show rule name="Terminal Racer"

# Add rule if missing
netsh advfirewall firewall add rule name="Terminal Racer" dir=in action=allow protocol=tcp localport=9999
```

---

#### 3. **Wrong IP Address**
**Verify Host IP:**

**On Host Machine (Linux):**
```bash
# Get all IPs
hostname -I

# Get specific interface
ip addr show

# Get public IP
curl ifconfig.me
```

**On Host Machine (Windows):**
```powershell
# Get all IPs
ipconfig

# Get public IP
(Invoke-WebRequest -Uri "https://ifconfig.me").Content
```

**Common IP Confusion:**
- `127.0.0.1` - Localhost (only works on same machine)
- `192.168.x.x` - Local network (LAN)
- `10.x.x.x` - Local network (LAN)
- `41.90.x.x` - Public IP (works across internet)

**Solution:**
- Use **public IP** (41.90.x.x) for remote connections
- Use **local IP** (192.168.x.x) for same network
- Verify with: `curl ifconfig.me`

---

#### 4. **Network Connectivity Between Devices**
**Test Connection (from Client):**

**Linux:**
```bash
# Test if host is reachable
ping 41.90.172.235

# Test if port is open
telnet 41.90.172.235 9999
# or
nc -zv 41.90.172.235 9999
```

**Windows:**
```powershell
# Test if host is reachable
ping 41.90.172.235

# Test if port is open
Test-NetConnection -ComputerName 41.90.172.235 -Port 9999
```

**Expected output for successful connection:**
```
Connected to 41.90.172.235.
Escape character is '^]'.
```

**If ping fails:**
- Devices not on same network
- ISP blocking ICMP
- Host machine offline
- Network cable disconnected

**If port test fails:**
- Firewall blocking port
- Host not listening
- Wrong port number

---

#### 5. **ISP/Router Blocking**
**Check if ISP blocks port 9999:**

**Solution:**
- Try different port (e.g., 8888, 5000)
- Use VPN on both machines
- Contact ISP about port blocking

---

## 🔧 Step-by-Step Debugging

### On Host Machine (Kisumu)

**Step 1: Check Prerequisites**
```bash
dotnet --version  # Should be 9.0.x
rustc --version   # Should be 1.90.x
```

**Step 2: Verify Port is Free**
```bash
# Check if 9999 is already in use
lsof -i :9999
# If output shows something, kill it
sudo kill -9 <PID>
```

**Step 3: Check Firewall**
```bash
# Disable firewall temporarily (for testing)
sudo ufw disable

# Or allow port
sudo ufw allow 9999/tcp
```

**Step 4: Get Your IP**
```bash
# Public IP
curl ifconfig.me

# Local IP
hostname -I
```

**Step 5: Run Game as Host**
```bash
cd Terminal_Racer/game-engine
LD_LIBRARY_PATH=../rust-renderer/target/release:$LD_LIBRARY_PATH dotnet run -c Release

# Select: 5 (Network Multiplayer)
# Select: h (Host)
# Note the displayed IP address
```

**Expected Output:**
```
╔════════════════════════════════════════╗
║   🌐 SERVER STARTED                    ║
╠════════════════════════════════════════╣
║ Port: 9999                             ║
║ Local IP: 41.90.172.235                ║
║ Status: Waiting for connection...      ║
╚════════════════════════════════════════╝
```

---

### On Client Machine (Nakuru)

**Step 1: Test Network Connection**
```bash
# Replace with host's IP
ping 41.90.172.235

# Test port specifically
nc -zv 41.90.172.235 9999
```

**Step 2: Run Game as Client**
```bash
cd Terminal_Racer/game-engine
LD_LIBRARY_PATH=../rust-renderer/target/release:$LD_LIBRARY_PATH dotnet run -c Release

# Select: 5 (Network Multiplayer)
# Select: j (Join)
# Enter: 41.90.172.235
```

**Expected Output (Success):**
```
╔════════════════════════════════════════╗
║   🌐 CONNECTING TO HOST                ║
╠════════════════════════════════════════╣
║ Host: 41.90.172.235                    ║
║ Port: 9999                             ║
╚════════════════════════════════════════╝

Resolving hostname: 41.90.172.235...
✓ Resolved to: 41.90.172.235

Attempt 1/5: Connecting... ✓ Connected!

✓ Successfully connected to 41.90.172.235:9999
🎮 Starting multiplayer game...
```

**Expected Output (Failure with Diagnostics):**
```
Attempt 1/5: Connecting... ❌ Failed: SocketException
Error: Connection refused
Retrying in 3 seconds...

Attempt 2/5: Connecting... ⏱️  Timeout (15s)
Retrying in 3 seconds...
```

---

## 📊 Common Error Messages & Fixes

| Error | Meaning | Solution |
|-------|---------|----------|
| `Connection refused` | Host not listening | Start host game, select Network Multiplayer |
| `Timeout (15s)` | No response from host | Check firewall, verify IP, check host is running |
| `SocketException` | Network error | Check network connection, try ping |
| `DNS resolution failed` | Can't resolve hostname | Verify IP format, check DNS settings |
| `No route to host` | Network unreachable | Check network connectivity, ISP blocking |

---

## 🚀 Quick Fix Checklist

- [ ] **Host**: Game running and in Network Multiplayer mode
- [ ] **Host**: Port 9999 is listening (`netstat -tuln | grep 9999`)
- [ ] **Host**: Firewall allows port 9999 (`sudo ufw allow 9999/tcp`)
- [ ] **Host**: Shared correct public IP with client
- [ ] **Client**: Can ping host (`ping 41.90.172.235`)
- [ ] **Client**: Can connect to port (`nc -zv 41.90.172.235 9999`)
- [ ] **Client**: Using correct IP address (not localhost)
- [ ] **Both**: Using same port (9999)
- [ ] **Both**: Network connection is stable
- [ ] **Both**: No VPN or proxy interfering

---

## 🔐 Advanced Debugging

### Enable Verbose Logging
```bash
# On client, capture connection attempts
strace -e trace=network dotnet run -c Release 2>&1 | grep -A5 "9999"
```

### Monitor Network Traffic
```bash
# On host, watch for incoming connections
sudo tcpdump -i any -n port 9999

# On client, watch for outgoing connections
sudo tcpdump -i any -n dst port 9999
```

### Test with netcat
```bash
# On host, listen on port 9999
nc -l 9999

# On client, try to connect
nc 41.90.172.235 9999
# Type something and press Enter
# If it appears on host, network is working
```

---

## 📞 If Still Not Working

**Gather this information:**
1. Output from `ping 41.90.172.235` on client
2. Output from `netstat -tuln | grep 9999` on host
3. Output from `sudo ufw status` on host
4. Full error message from game
5. Both public IPs (host and client)
6. ISP/Network provider name

**Then try:**
1. Restart both games
2. Restart both machines
3. Disable firewall temporarily
4. Use VPN on both machines
5. Try localhost (127.0.0.1) on same machine first to verify game works
6. Try different port (e.g., 8888)

---

## ✅ Verification Steps

**After fixing connection:**

1. **Host sees connection:**
   ```
   ✓ Player 2 connected from 192.168.1.100
   🎮 Starting multiplayer game...
   ```

2. **Client sees connection:**
   ```
   ✓ Successfully connected to 41.90.172.235:9999
   🎮 Starting multiplayer game...
   ```

3. **Both see game starting:**
   - Game HUD appears
   - Both players visible
   - Input responds on both sides

---

**Last Updated**: November 2025  
**Status**: Fully Tested ✓
