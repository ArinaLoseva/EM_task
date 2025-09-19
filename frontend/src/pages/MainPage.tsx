import React, { useRef, useState } from 'react';
import './MainPage.css';

const MainPage: React.FC = () => {
    const fileInputRef = useRef<HTMLInputElement>(null);
    const [result, setResult] = useState<string>('');
    const [location, setLocation] = useState<string>('');

    // открытие проводника
    const handleUploadButtonClick = () => {
        if (fileInputRef.current) {
            fileInputRef.current.click();
        }
    };

    // обработка выбора файла
    const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
        const files = event.target.files;
        if (!files || files.length === 0) return;

        const selectedFile = files[0];

        if (!selectedFile.name.toLowerCase().endsWith('.txt')) {
            setResult('Ошибка: пожалуйста, выберите файл с расширением .txt');
            return;
        }

        await uploadFile(selectedFile);
    };

    // отправка файла на сервер
    const uploadFile = async (file: File) => {
        try {
            const formData = new FormData();
            formData.append('file', file);

            const response = await fetch('http://localhost:5000/api/main/upload', {
                method: 'POST',
                body: formData,
            });

            if (response.ok) {
                const data = await response.json();
                setResult(`Файл успешно загружен: ${data.message}`);
            } else {
                setResult(`Ошибка загрузки: ${response.statusText}`);
            }
        } catch (error) {
            setResult(`Ошибка: ${error instanceof Error ? error.message : 'Неизвестная ошибка'}`);
        } 
    };

    // обработка поиска
    const handleSearch = async (event: React.FormEvent) => {
        event.preventDefault();

        if (!location.trim()) {
            setResult('Ошибка: введите локацию для поиска');
            return;
        }

        try {
            const response = await fetch('http://localhost:5000/api/main/search', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ location: location.trim() }),
            });

            if (response.ok) {
                const data = await response.json();
                setResult(data.result || 'Результаты поиска: ' + JSON.stringify(data));
            } else {
                setResult(`Ошибка поиска: ${response.statusText}`);
            }
        } catch (error) {
            setResult(`Ошибка: ${error instanceof Error ? error.message : 'Неизвестная ошибка'}`);
        } 
    };

    return (
        <div className="container">
            <div className="panel">
                <div className="info">

                    <input
                        type="file"
                        ref={fileInputRef}
                        onChange={handleFileChange}
                        accept=".txt"
                        style={{ display: 'none' }}
                    />

                    <button type="button" onClick={handleUploadButtonClick}>
                        Загрузить файл
                    </button>

                    <form onSubmit={handleSearch}>
                        <p>Введите локацию</p>
                        <input
                            type="text"
                            placeholder="/ru/svrd/..."
                            value={location}
                            onChange={(e) => setLocation(e.target.value)}
                        />
                        <button type="submit">
                            Поиск
                        </button>
                    </form>
                    <p>Результат:</p>
                    <p className="result">{result}</p>
                </div>
            </div>
        </div>
    );
};

export default MainPage;