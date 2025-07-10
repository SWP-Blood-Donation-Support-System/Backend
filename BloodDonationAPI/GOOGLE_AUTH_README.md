# Google Authentication API

## Cài đặt và cấu hình

### 1. Tạo Google OAuth2 Client
1. Truy cập [Google Cloud Console](https://console.cloud.google.com/)
2. Tạo project mới hoặc chọn project hiện có
3. Bật Google+ API
4. Tạo OAuth2 Client ID:
   - Application type: Web application
   - Authorized JavaScript origins: http://localhost:3000 (frontend URL)
   - Authorized redirect URIs: http://localhost:3000/auth/callback

### 2. Cấu hình appsettings.json
```json
{
  "GoogleAuth": {
    "ClientId": "371304679772-2hjjiqu8ek5hnj5feaf93ek80pbbg3do.apps.googleusercontent.com"
  }
}
```

## API Endpoint

### POST /api/User/google-login

**Request Body:**
```json
{
  "email": "user@gmail.com",
  "googleToken": "GOOGLE_ID_TOKEN_HERE"
}
```

**Response cho người dùng hiện có:**
```json
{
  "token": "JWT_TOKEN",
  "isNewUser": false,
  "message": "Đăng nhập thành công",
  "user": {
    "username": "user123",
    "email": "user@gmail.com",
    "role": "User",
    "fullName": "Nguyễn Văn A",
    "dateOfBirth": "1990-01-01",
    "gender": "Nam",
    "phone": "0123456789",
    "address": "123 ABC Street",
    "bloodType": "A+",
    "profileStatus": "Sẵn sàng hiến máu"
  }
}
```

**Response cho người dùng mới:**
```json
{
  "token": "JWT_TOKEN",
  "isNewUser": true,
  "message": "Tài khoản mới đã được tạo thành công",
  "user": {
    "username": "user123",
    "email": "user@gmail.com",
    "role": "User",
    "fullName": "Nguyễn Văn A",
    "dateOfBirth": null,
    "gender": "",
    "phone": "",
    "address": "",
    "bloodType": "",
    "profileStatus": "Chưa hoàn thành"
  },
  "additionalInfo": "Vui lòng hoàn thành thông tin cá nhân để sử dụng đầy đủ các tính năng"
}
```

## Frontend Integration - Chi tiết Implementation

### 1. HTML Setup (Vanilla JavaScript):
```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="UTF-8">
    <title>Google Login - Blood Donation</title>
    <script src="https://accounts.google.com/gsi/client" async defer></script>
    <style>
        .container { max-width: 400px; margin: 50px auto; padding: 20px; }
        .message { padding: 10px; margin: 10px 0; border-radius: 5px; }
        .success { background: #d4edda; color: #155724; }
        .error { background: #f8d7da; color: #721c24; }
        .loading { text-align: center; margin: 20px 0; }
        .custom-btn { 
            background: #4285f4; color: white; padding: 12px 24px; 
            border: none; border-radius: 5px; cursor: pointer; margin: 10px 0;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>🩸 Blood Donation System</h1>
        <h3>Đăng nhập bằng Google</h3>
        
        <!-- Google Auto Sign-In -->
        <div id="g_id_onload"
             data-client_id="371304679772-2hjjiqu8ek5hnj5feaf93ek80pbbg3do.apps.googleusercontent.com"
             data-callback="handleCredentialResponse"
             data-auto_prompt="false">
        </div>
        
        <!-- Google Sign-In Button -->
        <div class="g_id_signin" 
             data-type="standard"
             data-size="large"
             data-theme="outline"
             data-text="signin_with"
             data-shape="rectangular">
        </div>

        <!-- Custom Button -->
        <button onclick="googleSignIn()" class="custom-btn">
            🔐 Đăng nhập với Google
        </button>

        <!-- Loading & Messages -->
        <div id="loading" class="loading" style="display:none;">
            🔄 Đang xử lý đăng nhập...
        </div>
        <div id="message"></div>
    </div>

    <script>
        // Xử lý khi Google trả về credential
        function handleCredentialResponse(response) {
            console.log("✅ Google Token nhận được:", response.credential);
            
            // Parse token để lấy thông tin user (chỉ để log, không dùng để verify)
            try {
                const payload = JSON.parse(atob(response.credential.split('.')[1]));
                console.log("👤 User info từ Google:", {
                    email: payload.email,
                    name: payload.name,
                    picture: payload.picture
                });
                
                // Gửi token đến backend API
                loginWithGoogle(response.credential, payload.email);
            } catch (error) {
                console.error("❌ Lỗi parse token:", error);
                showMessage("Token không hợp lệ", "error");
            }
        }

        // Gửi Google token đến backend API
        async function loginWithGoogle(googleToken, email) {
            const API_BASE = 'https://localhost:7071'; // Thay đổi theo URL backend của bạn
            
            showLoading(true);
            clearMessage();
            
            try {
                console.log("🚀 Gửi request đến backend...");
                
                const response = await fetch(`${API_BASE}/api/User/google-login`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    },
                    body: JSON.stringify({
                        email: email,
                        googleToken: googleToken
                    })
                });

                console.log("📡 Response status:", response.status);
                const data = await response.json();
                console.log("📋 Response data:", data);

                if (response.ok) {
                    // ✅ Đăng nhập thành công
                    
                    // Lưu JWT token và thông tin user
                    localStorage.setItem('authToken', data.token);
                    localStorage.setItem('userInfo', JSON.stringify(data.user));
                    localStorage.setItem('isNewUser', data.isNewUser);
                    
                    // Hiển thị thông báo thành công
                    showMessage(`🎉 ${data.message}`, 'success');
                    
                    if (data.isNewUser) {
                        // 🆕 User mới - cần hoàn thiện thông tin
                        showMessage(`👋 Chào mừng ${data.user.fullName}! Vui lòng hoàn thiện thông tin cá nhân.`, 'success');
                        setTimeout(() => {
                            window.location.href = '/complete-profile.html';
                        }, 3000);
                    } else {
                        // 👤 User cũ - vào dashboard
                        showMessage(`🏠 Chào mừng ${data.user.fullName} quay lại!`, 'success');
                        setTimeout(() => {
                            window.location.href = '/dashboard.html';
                        }, 2000);
                    }
                } else {
                    // ❌ Lỗi từ server
                    showMessage(`❌ ${data.message || 'Đăng nhập thất bại'}`, 'error');
                }
                
            } catch (error) {
                console.error("💥 Network error:", error);
                showMessage('🔌 Lỗi kết nối đến server. Vui lòng kiểm tra lại.', 'error');
            } finally {
                showLoading(false);
            }
        }

        // Trigger Google Sign-In manually
        function googleSignIn() {
            if (window.google && window.google.accounts) {
                window.google.accounts.id.prompt();
            } else {
                showMessage('⏳ Google Sign-In chưa tải xong. Vui lòng thử lại.', 'error');
            }
        }

        // Utility functions
        function showLoading(show) {
            document.getElementById('loading').style.display = show ? 'block' : 'none';
        }

        function showMessage(message, type = 'info') {
            const msgElement = document.getElementById('message');
            msgElement.innerHTML = message;
            msgElement.className = `message ${type}`;
        }

        function clearMessage() {
            document.getElementById('message').innerHTML = '';
        }

        // Kiểm tra trạng thái đăng nhập khi load trang
        window.onload = function() {
            const token = localStorage.getItem('authToken');
            const userInfo = localStorage.getItem('userInfo');
            
            if (token && userInfo) {
                const user = JSON.parse(userInfo);
                console.log("🔑 User đã đăng nhập:", user);
                
                // Có thể tự động redirect hoặc hiển thị trạng thái
                showMessage(`✅ Đã đăng nhập: ${user.fullName}`, 'success');
                
                // Auto redirect (optional)
                // setTimeout(() => window.location.href = '/dashboard.html', 2000);
            }
        };

        // Handle logout (bonus)
        function logout() {
            localStorage.removeItem('authToken');
            localStorage.removeItem('userInfo');
            localStorage.removeItem('isNewUser');
            google.accounts.id.disableAutoSelect();
            showMessage('👋 Đã đăng xuất thành công', 'success');
            setTimeout(() => location.reload(), 1000);
        }
    </script>
</body>
</html>
```

### 2. React Implementation:
```jsx
// components/GoogleLogin.jsx
import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';

const GoogleLogin = () => {
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');
    const [messageType, setMessageType] = useState('info');
    const navigate = useNavigate();

    useEffect(() => {
        loadGoogleScript();
        
        // Cleanup function
        return () => {
            const script = document.querySelector('script[src*="gsi/client"]');
            if (script) {
                script.remove();
            }
        };
    }, []);

    const loadGoogleScript = () => {
        // Kiểm tra nếu script đã load
        if (window.google) {
            initializeGoogleSignIn();
            return;
        }

        const script = document.createElement('script');
        script.src = 'https://accounts.google.com/gsi/client';
        script.async = true;
        script.defer = true;
        
        script.onload = () => {
            console.log("✅ Google Script loaded");
            initializeGoogleSignIn();
        };

        script.onerror = () => {
            console.error("❌ Failed to load Google Script");
            setMessage("Không thể tải Google Sign-In. Vui lòng thử lại.");
            setMessageType("error");
        };

        document.head.appendChild(script);
    };

    const initializeGoogleSignIn = () => {
        window.google.accounts.id.initialize({
            client_id: '371304679772-2hjjiqu8ek5hnj5feaf93ek80pbbg3do.apps.googleusercontent.com',
            callback: handleCredentialResponse,
            auto_select: false,
            cancel_on_tap_outside: true
        });

        // Render button
        const buttonElement = document.getElementById('google-signin-button');
        if (buttonElement) {
            window.google.accounts.id.renderButton(buttonElement, {
                theme: 'outline',
                size: 'large',
                type: 'standard',
                text: 'signin_with',
                shape: 'rectangular',
                logo_alignment: 'left'
            });
        }
    };

    const handleCredentialResponse = async (response) => {
        console.log("🔑 Google credential received");
        
        try {
            // Decode payload để lấy email
            const payload = JSON.parse(atob(response.credential.split('.')[1]));
            console.log("👤 User từ Google:", payload);
            
            await loginWithGoogle(response.credential, payload.email);
        } catch (error) {
            console.error("❌ Error processing credential:", error);
            setMessage("Lỗi xử lý thông tin đăng nhập");
            setMessageType("error");
        }
    };

    const loginWithGoogle = async (googleToken, email) => {
        setLoading(true);
        setMessage('');

        try {
            const response = await fetch('https://localhost:7071/api/User/google-login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({
                    email: email,
                    googleToken: googleToken
                })
            });

            const data = await response.json();

            if (response.ok) {
                // Lưu token và thông tin user
                localStorage.setItem('authToken', data.token);
                localStorage.setItem('userInfo', JSON.stringify(data.user));
                
                setMessage(`🎉 ${data.message}`);
                setMessageType('success');
                
                // Redirect sau 2 giây
                setTimeout(() => {
                    if (data.isNewUser) {
                        navigate('/complete-profile');
                    } else {
                        navigate('/dashboard');
                    }
                }, 2000);
                
            } else {
                setMessage(data.message || 'Đăng nhập thất bại');
                setMessageType('error');
            }
        } catch (error) {
            console.error('❌ Login error:', error);
            setMessage('Lỗi kết nối đến server');
            setMessageType('error');
        } finally {
            setLoading(false);
        }
    };

    const handleCustomSignIn = () => {
        if (window.google && window.google.accounts) {
            window.google.accounts.id.prompt();
        } else {
            setMessage('Google Sign-In chưa sẵn sàng');
            setMessageType('error');
        }
    };

    return (
        <div className="max-w-md mx-auto mt-10 p-6 bg-white rounded-lg shadow-md">
            <h2 className="text-2xl font-bold text-center mb-6">
                🩸 Blood Donation Login
            </h2>
            
            {/* Google Sign-In Button Container */}
            <div id="google-signin-button" className="mb-4"></div>
            
            {/* Custom Button */}
            <button 
                onClick={handleCustomSignIn}
                disabled={loading}
                className="w-full bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded disabled:opacity-50"
            >
                {loading ? '🔄 Đang đăng nhập...' : '🔐 Đăng nhập với Google'}
            </button>

            {/* Message Display */}
            {message && (
                <div className={`mt-4 p-3 rounded ${
                    messageType === 'success' 
                        ? 'bg-green-100 text-green-700' 
                        : 'bg-red-100 text-red-700'
                }`}>
                    {message}
                </div>
            )}

            {/* Loading Spinner */}
            {loading && (
                <div className="mt-4 text-center">
                    <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-blue-500"></div>
                    <span className="ml-2">Đang xử lý...</span>
                </div>
            )}
        </div>
    );
};

export default GoogleLogin;
```

### 3. Complete Profile Component (React):
```jsx
// components/CompleteProfile.jsx
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

const CompleteProfile = () => {
    const [formData, setFormData] = useState({
        fullName: '',
        dateOfBirth: '',
        gender: '',
        phone: '',
        address: '',
        bloodType: ''
    });
    const [loading, setLoading] = useState(false);
    const [message, setMessage] = useState('');
    const navigate = useNavigate();

    useEffect(() => {
        // Load user info và pre-fill
        const userInfo = JSON.parse(localStorage.getItem('userInfo') || '{}');
        if (userInfo.fullName) {
            setFormData(prev => ({
                ...prev,
                fullName: userInfo.fullName
            }));
        }

        // Kiểm tra authentication
        const token = localStorage.getItem('authToken');
        if (!token) {
            navigate('/login');
        }
    }, [navigate]);

    const handleInputChange = (e) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value
        }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        setLoading(true);
        setMessage('');

        const token = localStorage.getItem('authToken');
        
        try {
            const response = await fetch('https://localhost:7071/api/User/update-profile', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify(formData)
            });

            const data = await response.json();

            if (response.ok) {
                // Cập nhật user info
                localStorage.setItem('userInfo', JSON.stringify(data.user));
                setMessage('✅ Hoàn thiện profile thành công!');
                
                setTimeout(() => {
                    navigate('/dashboard');
                }, 2000);
            } else {
                setMessage(`❌ ${data.message || 'Cập nhật thất bại'}`);
            }
        } catch (error) {
            setMessage('❌ Lỗi kết nối');
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="max-w-2xl mx-auto mt-10 p-6 bg-white rounded-lg shadow-md">
            <h2 className="text-2xl font-bold mb-6">📝 Hoàn thiện thông tin cá nhân</h2>
            
            <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                    <label className="block text-sm font-medium mb-1">Họ tên *</label>
                    <input
                        type="text"
                        name="fullName"
                        value={formData.fullName}
                        onChange={handleInputChange}
                        required
                        className="w-full p-2 border rounded"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Ngày sinh *</label>
                    <input
                        type="date"
                        name="dateOfBirth"
                        value={formData.dateOfBirth}
                        onChange={handleInputChange}
                        required
                        className="w-full p-2 border rounded"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Giới tính *</label>
                    <select
                        name="gender"
                        value={formData.gender}
                        onChange={handleInputChange}
                        required
                        className="w-full p-2 border rounded"
                    >
                        <option value="">Chọn giới tính</option>
                        <option value="Nam">Nam</option>
                        <option value="Nữ">Nữ</option>
                    </select>
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Số điện thoại *</label>
                    <input
                        type="tel"
                        name="phone"
                        value={formData.phone}
                        onChange={handleInputChange}
                        required
                        className="w-full p-2 border rounded"
                        placeholder="0xxx xxx xxx"
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Địa chỉ *</label>
                    <textarea
                        name="address"
                        value={formData.address}
                        onChange={handleInputChange}
                        required
                        rows="3"
                        className="w-full p-2 border rounded"
                        placeholder="Địa chỉ chi tiết..."
                    />
                </div>

                <div>
                    <label className="block text-sm font-medium mb-1">Nhóm máu *</label>
                    <select
                        name="bloodType"
                        value={formData.bloodType}
                        onChange={handleInputChange}
                        required
                        className="w-full p-2 border rounded"
                    >
                        <option value="">Chọn nhóm máu</option>
                        <option value="A+">A+</option>
                        <option value="A-">A-</option>
                        <option value="B+">B+</option>
                        <option value="B-">B-</option>
                        <option value="AB+">AB+</option>
                        <option value="AB-">AB-</option>
                        <option value="O+">O+</option>
                        <option value="O-">O-</option>
                    </select>
                </div>

                <button
                    type="submit"
                    disabled={loading}
                    className="w-full bg-red-500 hover:bg-red-600 text-white font-bold py-2 px-4 rounded disabled:opacity-50"
                >
                    {loading ? '🔄 Đang cập nhật...' : '✅ Hoàn thiện profile'}
                </button>
            </form>

            {message && (
                <div className={`mt-4 p-3 rounded ${
                    message.includes('✅') 
                        ? 'bg-green-100 text-green-700' 
                        : 'bg-red-100 text-red-700'
                }`}>
                    {message}
                </div>
            )}
        </div>
    );
};

export default CompleteProfile;
```

### 4. Protected Route Hook (React):
```jsx
// hooks/useAuth.js
import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';

export const useAuth = () => {
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);
    const navigate = useNavigate();

    useEffect(() => {
        checkAuth();
    }, []);

    const checkAuth = async () => {
        const token = localStorage.getItem('authToken');
        
        if (!token) {
            setLoading(false);
            return;
        }

        try {
            // Verify token với server
            const response = await fetch('https://localhost:7071/api/User/profile', {
                headers: {
                    'Authorization': `Bearer ${token}`
                }
            });

            if (response.ok) {
                const data = await response.json();
                setUser(data.user);
            } else {
                // Token không hợp lệ
                localStorage.removeItem('authToken');
                localStorage.removeItem('userInfo');
            }
        } catch (error) {
            console.error('Auth check failed:', error);
        } finally {
            setLoading(false);
        }
    };

    const logout = () => {
        localStorage.removeItem('authToken');
        localStorage.removeItem('userInfo');
        setUser(null);
        navigate('/login');
    };

    return { user, loading, logout, checkAuth };
};
```

### 5. Key Points cho Frontend Developer:

#### 🔧 **Setup Requirements:**
1. **Google Client ID**: Phải có Client ID từ Google Cloud Console
2. **CORS**: Backend phải cho phép frontend domain
3. **HTTPS**: Google Sign-In yêu cầu HTTPS (hoặc localhost)

#### 📝 **Flow hoạt động:**
1. **User click Google Sign-In** → Google hiển thị popup đăng nhập
2. **User nhập tài khoản Google** → Email và mật khẩu Google account
3. **User cấp quyền cho app** → Click "Cho phép" để app truy cập thông tin
4. **Google trả về ID Token** → Token chứa thông tin user đã verify
5. **Frontend gửi token đến backend** → Backend verify với Google API
6. **Backend tạo/tìm user** → Tạo user mới hoặc tìm user hiện có
7. **Backend trả về JWT token** → Frontend lưu vào localStorage
8. **Redirect theo trạng thái user** → New user → Complete profile, Existing user → Dashboard

#### 🔐 **Security Notes:**
- ✅ **Luôn verify token ở backend**, không tin tưởng frontend
- ✅ **JWT token có thời hạn**, cần handle token refresh
- ✅ **Logout cần clear localStorage** và disable Google auto-select
- ✅ **Protected routes** cần kiểm tra token validity

#### 🎯 **Best Practices:**
1. **Error Handling**: Xử lý đầy đủ các trường hợp lỗi
2. **Loading States**: Hiển thị loading khi đang xử lý
3. **User Feedback**: Thông báo rõ ràng cho user
4. **Responsive Design**: Đảm bảo UI tốt trên mobile
5. **Auto-redirect**: Tự động chuyển hướng dựa trên trạng thái user

#### 📱 **Testing:**
```javascript
// Test Google Login locally
// 1. Chạy backend: dotnet run
// 2. Mở frontend với HTTP server (không file://)
// 3. Test với Google account thực
console.log("Backend API:", "https://localhost:7071");
console.log("Google Client ID:", "371304679772-2hjjiqu8ek5hnj5feaf93ek80pbbg3do.apps.googleusercontent.com");
```
```html
<script src="https://accounts.google.com/gsi/client" async defer></script>

<div id="g_id_onload"
     data-client_id="YOUR_GOOGLE_CLIENT_ID"
     data-callback="handleCredentialResponse">
</div>
<div class="g_id_signin" data-type="standard"></div>

<script>
function handleCredentialResponse(response) {
    // response.credential contains the ID token
    const googleToken = response.credential;
    
    // Decode to get email (or send both token and email)
    const payload = JSON.parse(atob(googleToken.split('.')[1]));
    const email = payload.email;
    
    // Send to your API
    fetch('/api/User/google-login', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify({
            email: email,
            googleToken: googleToken
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.token) {
            // Store JWT token
            localStorage.setItem('token', data.token);
            
            if (data.isNewUser) {
                // Redirect to complete profile page
                window.location.href = '/complete-profile';
            } else {
                // Redirect to dashboard
                window.location.href = '/dashboard';
            }
        }
    });
}
</script>
```

## Features

1. **Token Verification**: Xác thực Google ID token với Google API
2. **User Creation**: Tự động tạo tài khoản mới cho người dùng lần đầu đăng nhập
3. **Username Generation**: Tự động tạo username duy nhất từ email
4. **JWT Integration**: Tích hợp với hệ thống JWT hiện có
5. **Profile Status**: Đánh dấu trạng thái profile cần hoàn thành cho user mới
6. **Error Handling**: Xử lý lỗi đầy đủ

## Security Features

- Google token verification với Google API
- Tạo random password cho Google users
- Kiểm tra email đã verify từ Google
- JWT token generation với thông tin user

## Database Changes

API này tương thích với cấu trúc database hiện có. Không cần thay đổi gì thêm.

### 6. Vue.js Implementation:
```vue
<!-- components/GoogleLogin.vue -->
<template>
  <div class="max-w-md mx-auto mt-10 p-6 bg-white rounded-lg shadow-md">
    <h2 class="text-2xl font-bold text-center mb-6">
      🩸 Blood Donation Login
    </h2>
    
    <!-- Google Sign-In Button -->
    <div id="google-signin-button" class="mb-4"></div>
    
    <!-- Custom Button -->
    <button 
      @click="handleCustomSignIn"
      :disabled="loading"
      class="w-full bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded disabled:opacity-50"
    >
      <span v-if="loading">🔄 Đang đăng nhập...</span>
      <span v-else>🔐 Đăng nhập với Google</span>
    </button>

    <!-- Message Display -->
    <div v-if="message" :class="messageClass" class="mt-4 p-3 rounded">
      {{ message }}
    </div>

    <!-- Loading Spinner -->
    <div v-if="loading" class="mt-4 text-center">
      <div class="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-blue-500"></div>
      <span class="ml-2">Đang xử lý...</span>
    </div>
  </div>
</template>

<script>
export default {
  name: 'GoogleLogin',
  data() {
    return {
      loading: false,
      message: '',
      messageType: 'info',
      GOOGLE_CLIENT_ID: '371304679772-2hjjiqu8ek5hnj5feaf93ek80pbbg3do.apps.googleusercontent.com',
      API_BASE: 'https://localhost:7071'
    }
  },
  computed: {
    messageClass() {
      return {
        'bg-green-100 text-green-700': this.messageType === 'success',
        'bg-red-100 text-red-700': this.messageType === 'error',
        'bg-blue-100 text-blue-700': this.messageType === 'info'
      }
    }
  },
  mounted() {
    this.loadGoogleScript();
  },
  methods: {
    loadGoogleScript() {
      // Check if Google script is already loaded
      if (window.google) {
        this.initializeGoogleSignIn();
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      
      script.onload = () => {
        console.log("✅ Google Script loaded");
        this.initializeGoogleSignIn();
      };

      script.onerror = () => {
        console.error("❌ Failed to load Google Script");
        this.showMessage("Không thể tải Google Sign-In", "error");
      };

      document.head.appendChild(script);
    },

    initializeGoogleSignIn() {
      window.google.accounts.id.initialize({
        client_id: this.GOOGLE_CLIENT_ID,
        callback: this.handleCredentialResponse,
        auto_select: false,
        cancel_on_tap_outside: true
      });

      // Render button
      const buttonElement = document.getElementById('google-signin-button');
      if (buttonElement) {
        window.google.accounts.id.renderButton(buttonElement, {
          theme: 'outline',
          size: 'large',
          type: 'standard',
          text: 'signin_with',
          shape: 'rectangular',
          logo_alignment: 'left'
        });
      }
    },

    handleCredentialResponse(response) {
      console.log("🔑 Google credential received");
      
      try {
        // Decode payload để lấy email
        const payload = JSON.parse(atob(response.credential.split('.')[1]));
        console.log("👤 User từ Google:", payload);
        
        this.loginWithGoogle(response.credential, payload.email);
      } catch (error) {
        console.error("❌ Error processing credential:", error);
        this.showMessage("Lỗi xử lý thông tin đăng nhập", "error");
      }
    },

    async loginWithGoogle(googleToken, email) {
      this.loading = true;
      this.message = '';

      try {
        const response = await fetch(`${this.API_BASE}/api/User/google-login`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          },
          body: JSON.stringify({
            email: email,
            googleToken: googleToken
          })
        });

        const data = await response.json();

        if (response.ok) {
          // Lưu token và thông tin user
          localStorage.setItem('authToken', data.token);
          localStorage.setItem('userInfo', JSON.stringify(data.user));
          
          this.showMessage(`🎉 ${data.message}`, 'success');
          
          // Redirect sau 2 giây
          setTimeout(() => {
            if (data.isNewUser) {
              this.$router.push('/complete-profile');
            } else {
              this.$router.push('/dashboard');
            }
          }, 2000);
          
        } else {
          this.showMessage(data.message || 'Đăng nhập thất bại', 'error');
        }
      } catch (error) {
        console.error('❌ Login error:', error);
        this.showMessage('Lỗi kết nối đến server', 'error');
      } finally {
        this.loading = false;
      }
    },

    handleCustomSignIn() {
      if (window.google && window.google.accounts) {
        window.google.accounts.id.prompt();
      } else {
        this.showMessage('Google Sign-In chưa sẵn sàng', 'error');
      }
    },

    showMessage(message, type = 'info') {
      this.message = message;
      this.messageType = type;
      
      // Auto clear message after 5 seconds
      setTimeout(() => {
        this.message = '';
      }, 5000);
    }
  }
}
</script>

<style scoped>
.animate-spin {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from { transform: rotate(0deg); }
  to { transform: rotate(360deg); }
}
</style>
```

### 7. Complete Profile Component (Vue.js):
```vue
<!-- components/CompleteProfile.vue -->
<template>
  <div class="max-w-2xl mx-auto mt-10 p-6 bg-white rounded-lg shadow-md">
    <h2 class="text-2xl font-bold mb-6">📝 Hoàn thiện thông tin cá nhân</h2>
    
    <form @submit.prevent="handleSubmit" class="space-y-4">
      <div>
        <label class="block text-sm font-medium mb-1">Họ tên *</label>
        <input
          v-model="formData.fullName"
          type="text"
          required
          class="w-full p-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div>
        <label class="block text-sm font-medium mb-1">Ngày sinh *</label>
        <input
          v-model="formData.dateOfBirth"
          type="date"
          required
          class="w-full p-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
        />
      </div>

      <div>
        <label class="block text-sm font-medium mb-1">Giới tính *</label>
        <select
          v-model="formData.gender"
          required
          class="w-full p-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">Chọn giới tính</option>
          <option value="Nam">Nam</option>
          <option value="Nữ">Nữ</option>
        </select>
      </div>

      <div>
        <label class="block text-sm font-medium mb-1">Số điện thoại *</label>
        <input
          v-model="formData.phone"
          type="tel"
          required
          class="w-full p-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="0xxx xxx xxx"
        />
      </div>

      <div>
        <label class="block text-sm font-medium mb-1">Địa chỉ *</label>
        <textarea
          v-model="formData.address"
          required
          rows="3"
          class="w-full p-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="Địa chỉ chi tiết..."
        />
      </div>

      <div>
        <label class="block text-sm font-medium mb-1">Nhóm máu *</label>
        <select
          v-model="formData.bloodType"
          required
          class="w-full p-2 border rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
        >
          <option value="">Chọn nhóm máu</option>
          <option value="A+">A+</option>
          <option value="A-">A-</option>
          <option value="B+">B+</option>
          <option value="B-">B-</option>
          <option value="AB+">AB+</option>
          <option value="AB-">AB-</option>
          <option value="O+">O+</option>
          <option value="O-">O-</option>
        </select>
      </div>

      <button
        type="submit"
        :disabled="loading"
        class="w-full bg-red-500 hover:bg-red-600 text-white font-bold py-2 px-4 rounded disabled:opacity-50 transition-colors"
      >
        <span v-if="loading">🔄 Đang cập nhật...</span>
        <span v-else>✅ Hoàn thiện profile</span>
      </button>
    </form>

    <div v-if="message" :class="messageClass" class="mt-4 p-3 rounded">
      {{ message }}
    </div>
  </div>
</template>

<script>
export default {
  name: 'CompleteProfile',
  data() {
    return {
      formData: {
        fullName: '',
        dateOfBirth: '',
        gender: '',
        phone: '',
        address: '',
        bloodType: ''
      },
      loading: false,
      message: '',
      messageType: 'info',
      API_BASE: 'https://localhost:7071'
    }
  },
  computed: {
    messageClass() {
      return {
        'bg-green-100 text-green-700': this.messageType === 'success',
        'bg-red-100 text-red-700': this.messageType === 'error',
        'bg-blue-100 text-blue-700': this.messageType === 'info'
      }
    }
  },
  mounted() {
    this.loadUserInfo();
    this.checkAuth();
  },
  methods: {
    loadUserInfo() {
      const userInfo = JSON.parse(localStorage.getItem('userInfo') || '{}');
      if (userInfo.fullName) {
        this.formData.fullName = userInfo.fullName;
      }
    },

    checkAuth() {
      const token = localStorage.getItem('authToken');
      if (!token) {
        this.$router.push('/login');
      }
    },

    async handleSubmit() {
      this.loading = true;
      this.message = '';

      const token = localStorage.getItem('authToken');
      
      try {
        const response = await fetch(`${this.API_BASE}/api/User/update-profile`, {
          method: 'PUT',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`
          },
          body: JSON.stringify(this.formData)
        });

        const data = await response.json();

        if (response.ok) {
          // Cập nhật user info
          localStorage.setItem('userInfo', JSON.stringify(data.user));
          this.showMessage('✅ Hoàn thiện profile thành công!', 'success');
          
          setTimeout(() => {
            this.$router.push('/dashboard');
          }, 2000);
        } else {
          this.showMessage(`❌ ${data.message || 'Cập nhật thất bại'}`, 'error');
        }
      } catch (error) {
        this.showMessage('❌ Lỗi kết nối', 'error');
      } finally {
        this.loading = false;
      }
    },

    showMessage(message, type = 'info') {
      this.message = message;
      this.messageType = type;
    }
  }
}
</script>
```

### 8. Troubleshooting Guide:

#### ❌ **Common Issues & Solutions:**

**1. "Google Sign-In không load"**
```javascript
// Giải pháp:
// - Kiểm tra HTTPS/localhost
// - Kiểm tra Client ID đúng
// - Kiểm tra domain được whitelist trong Google Console

// Debug:
console.log("Google script loaded:", !!window.google);
console.log("Client ID:", YOUR_CLIENT_ID);
```

**2. "Invalid token" hoặc "Token expired"**
```javascript
// Giải pháp:
// - Verify token ngay lập tức khi nhận được
// - Kiểm tra thời gian hệ thống
// - Refresh token nếu cần

// Debug:
const payload = JSON.parse(atob(token.split('.')[1]));
console.log("Token exp:", new Date(payload.exp * 1000));
console.log("Current time:", new Date());
```

**3. "CORS Error"**
```csharp
// Backend: thêm vào Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder =>
    {
        builder.WithOrigins("https://yourdomain.com", "http://localhost:3000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

app.UseCors("AllowSpecificOrigin");
```

**4. "User already exists with different login method"**
```javascript
// Giải pháp:
// - Kiểm tra email trùng lặp
// - Merge account hoặc show error message
// - Implement account linking

// Backend sẽ trả về error message cụ thể
```

**5. "JWT Token không hợp lệ"**
```javascript
// Giải pháp:
// - Kiểm tra Authorization header format
// - Verify token với server
// - Refresh token nếu expired

// Correct format:
headers: {
    'Authorization': `Bearer ${token}`
}
```

#### 🔧 **Debug Tools:**

```javascript
// 1. Token Decoder
function decodeJWT(token) {
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        console.log("JWT Payload:", payload);
        return payload;
    } catch (e) {
        console.error("Invalid JWT:", e);
        return null;
    }
}

// 2. Google Token Decoder
function decodeGoogleToken(token) {
    try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        console.log("Google Token Info:", {
            email: payload.email,
            name: payload.name,
            picture: payload.picture,
            exp: new Date(payload.exp * 1000)
        });
        return payload;
    } catch (e) {
        console.error("Invalid Google token:", e);
        return null;
    }
}

// 3. API Test Function
async function testAPI() {
    const token = localStorage.getItem('authToken');
    try {
        const response = await fetch('/api/User/profile', {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        console.log("API Response:", response.status, await response.json());
    } catch (error) {
        console.error("API Error:", error);
    }
}
```

## 🔐 **Quy trình đăng nhập Google chi tiết**

### 🎯 **Flow đăng nhập Google hoàn chỉnh:**

#### **Bước 1: User click "Đăng nhập với Google"**
```javascript
// Khi user click button này:
<button onclick="googleSignIn()">🔐 Đăng nhập với Google</button>
```

#### **Bước 2: Google hiển thị popup/redirect đăng nhập**
- Google mở popup window hoặc redirect đến trang đăng nhập Google
- User thấy giao diện đăng nhập chính thức của Google

#### **Bước 3: User nhập thông tin đăng nhập Google**
```
┌─────────────────────────────────────┐
│        🔐 Đăng nhập Google          │
├─────────────────────────────────────┤
│ Email: user@gmail.com               │
│ Password: ************              │
│                                     │
│ [ ] Ghi nhớ tôi                     │
│                                     │
│        [Tiếp theo]                  │
└─────────────────────────────────────┘
```

**User phải nhập thông tin Google Account của họ:**
- ✅ **Email Google** (ví dụ: `user@gmail.com`) - Tài khoản Gmail hiện có
- ✅ **Mật khẩu Google** (mật khẩu tài khoản Google của họ) - Để xác thực danh tính
- ✅ **2FA nếu có** (SMS, Authenticator app, v.v.) - Bảo mật bổ sung

💡 **Lưu ý quan trọng**: User phải có tài khoản Google/Gmail để đăng nhập. Nếu chưa có, họ cần tạo tài khoản Google trước.

#### **Bước 4: Google yêu cầu cấp quyền**
```
┌─────────────────────────────────────┐
│    🩸 Blood Donation System         │
│    muốn truy cập tài khoản Google   │
├─────────────────────────────────────┤
│ Ứng dụng này sẽ có thể:             │
│ ✓ Xem địa chỉ email                 │
│ ✓ Xem thông tin cá nhân cơ bản      │
│                                     │
│ Tài khoản: user@gmail.com           │
│                                     │
│   [Hủy]        [Cho phép]          │
└─────────────────────────────────────┘
```

#### **Bước 5: Google trả về token cho app**
```javascript
// Sau khi user click "Cho phép", Google gọi callback:
function handleCredentialResponse(response) {
    const googleToken = response.credential; // JWT token từ Google
    
    // Token chứa thông tin user đã được Google verify:
    const payload = JSON.parse(atob(googleToken.split('.')[1]));
    console.log("Thông tin từ Google:", {
        email: payload.email,           // user@gmail.com
        name: payload.name,             // "Nguyễn Văn A"
        picture: payload.picture,       // URL avatar
        email_verified: payload.email_verified // true
    });
    
    // Gửi token đến backend để xử lý
    loginWithGoogle(googleToken, payload.email);
}
```

#### **Bước 6: Backend xử lý token**
```csharp
[HttpPost("google-login")]
public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDto dto)
{
    // 1. Verify token với Google API
    var payload = await GoogleJsonWebSignature.ValidateAsync(dto.GoogleToken);
    
    // 2. Kiểm tra user đã tồn tại chưa
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);
    
    if (user == null) {
        // 3. Tạo user mới nếu chưa có
        user = new User {
            Email = payload.Email,
            FullName = payload.Name,
            Username = GenerateUsername(payload.Email),
            Password = GenerateRandomPassword(), // Random password
            ProfileStatus = "Chưa hoàn thành"
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    }
    
    // 4. Tạo JWT token cho hệ thống
    var jwtToken = GenerateJwtToken(user);
    
    return Ok(new {
        token = jwtToken,
        isNewUser = user.ProfileStatus == "Chưa hoàn thành",
        user = user
    });
}
```

### 🎯 **Kịch bản cụ thể:**

#### **Scenario 1: User lần đầu đăng nhập (Tạo tài khoản mới)**
```
1. User: Click "Đăng nhập với Google"
2. Google: Hiển thị popup đăng nhập Google
3. User: Nhập email Google "john@gmail.com" 
4. User: Nhập mật khẩu tài khoản Google
5. User: Hoàn thành 2FA (nếu có)
6. Google: Yêu cầu cấp quyền cho Blood Donation App
7. User: Click "Cho phép" để app truy cập thông tin cơ bản
8. Google: Trả về token chứa thông tin John đã verify
9. Backend: Không tìm thấy user với email "john@gmail.com"
10. Backend: TỰ ĐỘNG tạo tài khoản mới với:
    - Email: "john@gmail.com" (từ Google)
    - FullName: "John Doe" (từ Google)
    - Username: "john_doe_123" (tự tạo)
    - Password: "random_generated_password" (tự tạo)
    - ProfileStatus: "Chưa hoàn thành"
11. Backend: Trả về JWT token và isNewUser: true
12. Frontend: Redirect đến trang hoàn thiện thông tin
```

#### **Scenario 2: User đã có tài khoản (Đăng nhập lại)**
```
1. User: Click "Đăng nhập với Google" 
2. Google: Hiển thị popup (có thể auto-login nếu đã đăng nhập)
3. User: Chọn account hoặc nhập lại password Google
4. Google: Trả về token
5. Backend: Tìm thấy user với email này trong database
6. Backend: Trả về JWT token và isNewUser: false
7. Frontend: Redirect đến dashboard
```

### 🔒 **Bảo mật và lưu ý:**

#### **Về mật khẩu:**
- ✅ **User chỉ nhập mật khẩu Google** (không phải mật khẩu của app)
- ✅ **Backend tự tạo random password** cho user trong database
- ✅ **User không bao giờ biết password này** - chỉ đăng nhập qua Google
- ✅ **Không thể đăng nhập trực tiếp** với username/password

#### **Về quyền riêng tư:**
- ✅ **Google chỉ chia sẻ thông tin cơ bản** (email, tên, avatar)
- ✅ **User có thể revoke quyền** bất kỳ lúc nào
- ✅ **Không lưu mật khẩu Google** trong hệ thống

### 🎯 **UI/UX Flow:**

#### **Giao diện đăng nhập:**
```html
<!-- Trang đăng nhập của app -->
<div class="login-container">
    <h2>🩸 Blood Donation System</h2>
    <p>Đăng nhập để tham gia hiến máu cứu người</p>
    
    <!-- Google Sign-In Button -->
    <button onclick="googleSignIn()" class="google-btn">
        <img src="google-icon.png" alt="Google">
        Đăng nhập với Google
    </button>
    
    <p class="note">
        Bằng cách đăng nhập, bạn đồng ý với điều khoản sử dụng
    </p>
</div>
```

#### **Sau khi đăng nhập:**
```javascript
// Nếu user mới (isNewUser: true):
localStorage.setItem('authToken', data.token);
showMessage('Chào mừng! Vui lòng hoàn thiện thông tin để bắt đầu.');
setTimeout(() => {
    window.location.href = '/complete-profile';
}, 2000);

// Nếu user cũ (isNewUser: false):
localStorage.setItem('authToken', data.token);
showMessage('Chào mừng bạn trở lại!');
setTimeout(() => {
    window.location.href = '/dashboard';
}, 2000);
```

### 🚨 **Lưu ý quan trọng:**

#### **User cần hiểu:**
1. **Phải có tài khoản Google/Gmail** - Không có tài khoản Google thì không thể đăng nhập
2. **Nhập email và mật khẩu Google** - Đăng nhập bằng thông tin Google hiện có
3. **App tự động tạo tài khoản** - Không cần đăng ký riêng cho Blood Donation System
4. **Không tạo mật khẩu mới** - App tự tạo password random trong database
5. **Thông tin cơ bản từ Google** - Email, tên đã có sẵn
6. **Cần hoàn thiện thông tin** - SĐT, địa chỉ, nhóm máu cần nhập thêm

#### **Bảo mật:**
- ✅ **Chỉ Google xử lý password** - App không bao giờ thấy
- ✅ **Token có thời hạn** - Tự động hết hiệu lực
- ✅ **Có thể revoke quyền** từ Google Account Settings
- ✅ **HTTPS required** - Không hoạt động trên HTTP

### 📱 **Hướng dẫn cho User:**

```markdown
## Cách đăng nhập và tạo tài khoản:

### Bước 1: Chuẩn bị
- ✅ Đảm bảo bạn có tài khoản Google/Gmail (ví dụ: yourname@gmail.com)
- ✅ Nhớ mật khẩu tài khoản Google của bạn
- ✅ Chuẩn bị 2FA nếu bạn có bật (Authenticator app, SMS)

### Bước 2: Đăng nhập
1. **Click "Đăng nhập với Google"** trên trang Blood Donation
2. **Nhập email Gmail** của bạn (ví dụ: yourname@gmail.com)
3. **Nhập mật khẩu Gmail** (mật khẩu tài khoản Google)
4. **Hoàn thành 2FA** nếu bạn có bật
5. **Click "Cho phép"** để cấp quyền cho app truy cập thông tin cơ bản

### Bước 3: Hoàn thiện (chỉ lần đầu)
6. **Hệ thống tự động tạo tài khoản** cho bạn
7. **Hoàn thiện thông tin** cần thiết:
   - Ngày sinh
   - Giới tính  
   - Số điện thoại
   - Địa chỉ
   - Nhóm máu

## Lưu ý quan trọng:
- ✅ **Cần có tài khoản Google** - Nếu chưa có, tạo tại accounts.google.com
- ✅ **Tự động tạo tài khoản** - Không cần đăng ký riêng
- ✅ **Bảo mật cao** - Thông tin được bảo vệ bởi Google
- ✅ **Có thể hủy quyền** - Revoke từ Google Account Settings bất kỳ lúc nào
```
