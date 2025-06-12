import axiosInstance from "./axiosConfig";
import { v4 as uuidv4 } from "uuid";

class ChatbotService {
	constructor() {
		this.conversationHistory = [];
		this.sessionId = uuidv4();
	}

	// Gửi tin nhắn tới Backend API
	async sendMessage(message, productContext = null) {
		try {
			const response = await axiosInstance.post("/chatbot/message", {
				message: message,
				sessionId: this.sessionId,
				context: productContext,
			});

			if (response.data && response.data.success) {
				const aiResponse = response.data.data;

				// Lưu lịch sử hội thoại
				this.conversationHistory.push(
					{ role: "user", content: message, timestamp: new Date() },
					{
						role: "assistant",
						content: aiResponse.content,
						timestamp: new Date(aiResponse.timestamp),
					}
				);

				// Giới hạn lịch sử để tránh quá tải
				if (this.conversationHistory.length > 20) {
					this.conversationHistory =
						this.conversationHistory.slice(-20);
				}

				return {
					id: aiResponse.id,
					content: aiResponse.content,
					timestamp: new Date(aiResponse.timestamp),
					isUser: false,
					suggestedActions: aiResponse.suggestedActions || [],
				};
			}

			throw new Error("Invalid response from server");
		} catch (error) {
			console.error("Error calling Chatbot API:", error);

			// Fallback response
			return {
				id: uuidv4(),
				content: this.getFallbackResponse(message),
				timestamp: new Date(),
				isUser: false,
				isError: true,
			};
		}
	}

	// Lấy gợi ý câu hỏi thường gặp từ backend
	async getQuickQuestions() {
		try {
			const response = await axiosInstance.get(
				"/chatbot/quick-questions"
			);
			if (response.data && response.data.success) {
				return response.data.data;
			}
		} catch (error) {
			console.error("Error fetching quick questions:", error);
		}

		// Fallback questions
		return [
			"Làm sao để đặt hàng?",
			"Chính sách đổi trả như thế nào?",
			"Thời gian giao hàng bao lâu?",
			"Có những phương thức thanh toán nào?",
			"Làm sao để theo dõi đơn hàng?",
			"Tư vấn sản phẩm phù hợp với tôi",
		];
	}

	// Lấy context sản phẩm từ backend
	async getProductContext() {
		try {
			const response = await axiosInstance.post("/chatbot/context");
			if (response.data && response.data.success) {
				return response.data.data;
			}
		} catch (error) {
			console.error("Error fetching product context:", error);
		}
		return null;
	}

	// Xóa lịch sử hội thoại
	clearHistory() {
		this.conversationHistory = [];
		this.sessionId = uuidv4();
	}

	// Fallback response khi API không khả dụng
	getFallbackResponse(message) {
		const lowerMessage = message.toLowerCase();

		if (lowerMessage.includes("đặt hàng") || lowerMessage.includes("mua")) {
			return "Để đặt hàng, bạn có thể thêm sản phẩm vào giỏ hàng và tiến hành thanh toán. Tôi có thể hướng dẫn bạn chi tiết hơn không?";
		}

		if (
			lowerMessage.includes("giao hàng") ||
			lowerMessage.includes("ship")
		) {
			return 'Thời gian giao hàng thường từ 1-3 ngày tùy theo khu vực. Bạn có thể theo dõi đơn hàng trong mục "Đơn hàng của tôi".';
		}

		if (lowerMessage.includes("thanh toán")) {
			return "SmartBuy hỗ trợ thanh toán COD, chuyển khoản, ví điện tử và thẻ tín dụng. Bạn muốn biết chi tiết phương thức nào?";
		}

		return "Xin chào! Tôi là trợ lý SmartBuy. Tôi có thể giúp bạn tư vấn sản phẩm, hướng dẫn đặt hàng và giải đáp các thắc mắc. Bạn cần hỗ trợ gì?";
	}
}

export default new ChatbotService();
