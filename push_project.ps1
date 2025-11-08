<#
push_project.ps1
Single script that re-launches itself under ExecutionPolicy Bypass if needed,
then performs git init/commit/pull/push to origin Web-System.

Save as C:\TransnetAEX\push_project.ps1
#>

# ---------- CONFIG ----------
$Folder  = "C:\TransnetAEX"
$Remote  = "https://github.com/SimphiweT/TransnetAEX.git"  # repo (not /tree/...)
$Branch  = "Web-System"
$Message = "Initial push of TransnetAEX to Web-System"
# -----------------------------

function Test-Bypass {
    # returns $true if current process already has a permissive exec policy
    $proc = (Get-ExecutionPolicy -Scope Process -ErrorAction SilentlyContinue)
    $cu   = (Get-ExecutionPolicy -Scope CurrentUser -ErrorAction SilentlyContinue)
    $lm   = (Get-ExecutionPolicy -Scope LocalMachine -ErrorAction SilentlyContinue)

    # prefer Process when present
    $effective = $proc
    if (-not $effective) { $effective = $cu }
    if (-not $effective) { $effective = $lm }

    if ($null -eq $effective) { return $false }
    return $effective -in @('Bypass','Unrestricted','RemoteSigned')
}

# If not running permissively, re-launch this script with -ExecutionPolicy Bypass
if (-not (Test-Bypass)) {
    Write-Host "Current execution policy is restrictive. Attempting to re-launch this script with -ExecutionPolicy Bypass..."
    # find the PowerShell executable (prefer pwsh if available)
    $pwsh = (Get-Command pwsh -ErrorAction SilentlyContinue).Source
    if (-not $pwsh) { $pwsh = (Get-Command powershell -ErrorAction SilentlyContinue).Source }

    if (-not $pwsh) {
        Write-Error "No PowerShell executable was found on PATH. Please install PowerShell or run the script from a PowerShell prompt."
        exit 1
    }

    # Build argument list
    $scriptPath = $PSCommandPath
    if (-not $scriptPath) {
        Write-Error "Cannot determine script path (`$PSCommandPath is empty). Run the script using: powershell -NoProfile -ExecutionPolicy Bypass -File `"<path-to-script>`""
        exit 1
    }

    $argsList = @(
        '-NoProfile'
        '-ExecutionPolicy', 'Bypass'
        '-File', $scriptPath
    )
    # Forward any original args to the re-launched instance
    if ($Args.Count -gt 0) { $argsList += $Args }

    try {
        # Start a new process with the bypass flag (non-elevated). This avoids modifying machine policy.
        Write-Host "Launching: $pwsh $($argsList -join ' ')"
        $proc = Start-Process -FilePath $pwsh -ArgumentList $argsList -PassThru -Wait -ErrorAction Stop
        # relay child exit code
        exit $proc.ExitCode
    } catch {
        Write-Warning "Could not re-launch script automatically. Error: $($_.Exception.Message)"
        Write-Host ""
        Write-Host "You can run the script manually with a bypassed execution policy using this command (one-time):"
        Write-Host ""
        if ($pwsh -match 'pwsh') {
            Write-Host "pwsh -NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`""
        } else {
            Write-Host "powershell -NoProfile -ExecutionPolicy Bypass -File `"$scriptPath`""
        }
        Write-Host ""
        Write-Host "Or run in current session (temporary, no admin needed):"
        Write-Host "Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass; & `"$scriptPath`""
        exit 2
    }
}

# --- At this point script is running under a permissive policy (Process=Bypass or similar) ---
Write-Host "Running under permissive execution policy. Continuing..."

# ---------- Begin main operations ----------
try {
    Set-Location -Path $Folder -ErrorAction Stop
} catch {
    Write-Error "Folder not found: $Folder"
    exit 1
}

# Ensure git is available
if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Error "Git not found on PATH. Install Git or add it to PATH then re-run."
    exit 1
}

# Initialize repo if necessary
if (-not (Test-Path ".git")) {
    Write-Host "Initializing git repository..."
    git init
}

# Ensure remote 'origin' points to correct URL
$remotes = git remote -v 2>$null
if ($remotes -match "^origin\s+") {
    Write-Host "Updating existing 'origin' URL to $Remote"
    git remote set-url origin $Remote
} else {
    Write-Host "Adding remote 'origin' -> $Remote"
    git remote add origin $Remote
}

# Fetch remote refs (safe)
Write-Host "Fetching origin..."
git fetch origin --prune

# Checkout/create local branch with same name as target remote branch
Write-Host "Checking out local branch '$Branch'"
git checkout -B $Branch

# Create .gitignore if missing (optional)
if (-not (Test-Path ".gitignore")) {
    @"
bin/
obj/
node_modules/
*.user
*.suo
.vs/
.publish/
dist/
"@ | Out-File -FilePath .gitignore -Encoding UTF8
    git add .gitignore | Out-Null
    # commit only if above add created a change
    if (git diff --cached --name-only) { git commit -m "Add default .gitignore" }
}

# Stage & commit
Write-Host "Staging changes..."
git add -A
$staged = git diff --cached --name-only
if ($staged) {
    Write-Host "Committing changes..."
    git commit -m $Message
} else {
    Write-Host "Nothing to commit (working tree clean)."
}

# Pull remote branch to reduce non-fast-forward errors; prefer local changes on conflict
Write-Host "Pulling origin/$Branch (preferring local changes on conflicts)..."
try {
    git pull origin $Branch --no-rebase -s recursive -X ours --allow-unrelated-histories
} catch {
    Write-Warning "git pull failed or returned non-zero. You may need to resolve conflicts manually."
}

# Push and set upstream
Write-Host "Pushing to origin/$Branch..."
try {
    git push -u origin $Branch
    Write-Host "Push complete. Branch '$Branch' is now on origin."
} catch {
    Write-Error "Push failed. If you really want to overwrite remote branch, run: git push -u origin $Branch --force"
    exit 1
}

# End
Write-Host "All done."
exit 0
